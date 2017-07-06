using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Speech.Tts;
using Android.Speech;
using System.Collections.Generic;
using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
using Android.Provider;
using Android.Media;

namespace GDSSAssistant.Activities
{
    [Activity(Label = "GDSS: SMS Reader")]
    public class MessageReaderActivity : Activity, TextToSpeech.IOnInitListener//, AudioManager.IOnAudioFocusChangeListener
    {
        private TextToSpeech textToSpeech;
        //Context context;
        private string ActivityStatus;
        private readonly int NeedLang = 103;
        //private bool NeedToSpeak;
        //private string MessageToSpeak;
        private Java.Util.Locale lang;
        private int curPos;
        private int maxPos;

        //int count = 1;
        private bool isRecording;
        private readonly int VOICE = 10;
        //private TextView STTtextBox;
        //private ImageButton recButton;
        //private ImageButton callButton;
        private Button btnSayIt;

        private string SMSFrom;
        private string SMSDate;
        private string SMSID;
        private string SMSMessage;

        // Interface method required for IOnInitListener
        void TextToSpeech.IOnInitListener.OnInit(OperationResult status)
        {
            // if we get an error, default to the default language
            if (status == OperationResult.Error)
                textToSpeech.SetLanguage(Java.Util.Locale.Default);
            // if the listener is ok, set the lang
            if (status == OperationResult.Success)
                textToSpeech.SetLanguage(lang);
        }


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.MessageReader);

            // set the isRecording flag to false (not recording)
            isRecording = false;

            SMSFrom = Intent.GetStringExtra("SMSFrom") ?? "From not available";
            SMSDate = Intent.GetStringExtra("SMSDate") ?? "Date not available";
            SMSID = Intent.GetStringExtra("SMSID") ?? "ID not available";
            SMSMessage = Intent.GetStringExtra("SMSMessage") ?? "Message not available";
            curPos = Intent.GetIntExtra("curPos",0);
            maxPos = Intent.GetIntExtra("maxPos", 0);
            //var autoRead = Intent.GetIntExtra("autoRead", 0);

            var tvFrom = FindViewById<TextView>(Resource.Id.tvFromSMSReader);
            tvFrom.Text = SMSFrom;

            var tvDate = FindViewById<TextView>(Resource.Id.tvDateSMSReader);
            tvDate.Text = SMSDate;

            var tvMessage = FindViewById<TextView>(Resource.Id.tvMessageSMSReader);
            tvMessage.Text = SMSMessage;

            //initialize text to speech (read/speak)
            InitTTSMethod();

            var backButton = FindViewById<Button>(Resource.Id.btnBackSMSReader);
            backButton.Click += delegate {
                Finish();
            };

            var btnStopSpeak = FindViewById<Button>(Resource.Id.btnStopSMSReader);
            btnStopSpeak.Click += delegate {
                if (textToSpeech.IsSpeaking)
                {
                    textToSpeech.Stop();
                }
            };

            var btnPrev = FindViewById<Button>(Resource.Id.btnReadPrevSMSReader);
            btnPrev.Click += delegate {
                GetSMSMessageByPosition(curPos -1);
            };

            var btnNextSMS = FindViewById<Button>(Resource.Id.btnReadNextSMSReader);
            btnNextSMS.Click += delegate {
                GetSMSMessageByPosition(curPos + 1);
            };

            ActivityStatus = "Created";
            //var autoRead = Intent.GetIntExtra("autoRead", 0);
            //if (autoRead == 1) {

            //    var btnReadSMSReader = FindViewById<Button>(Resource.Id.btnReadSMSReader);
            //    btnReadSMSReader.PerformClick();
            //}
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (textToSpeech.IsSpeaking)
            {
                textToSpeech.Stop();
            }
        }

        protected override void OnActivityResult(int req, Result res, Intent data)
        {

            if (req == NeedLang)
            {
                // we need a new language installed
                var installTTS = new Intent();
                installTTS.SetAction(TextToSpeech.Engine.ActionInstallTtsData);
                StartActivity(installTTS);
            }
            base.OnActivityResult(req, res, data);
        }
        
        #region "Initialize Text-To-Speech"
        private void InitTTSMethod()
        {
            btnSayIt = FindViewById<Button>(Resource.Id.btnReadSMSReader);

            // set up the TextToSpeech object
            // third parameter is the speech engine to use
            textToSpeech = new TextToSpeech(this, this, "com.google.android.tts");

            // set up the langauge spinner
            // set the top option to be default
            var langAvailable = new List<string> { "Default" };

            // our spinner only wants to contain the languages supported by the tts and ignore the rest
            var localesAvailable = Java.Util.Locale.GetAvailableLocales().ToList();
            foreach (var locale in localesAvailable)
            {
                LanguageAvailableResult res = textToSpeech.IsLanguageAvailable(locale);
                switch (res)
                {
                    case LanguageAvailableResult.Available:
                        langAvailable.Add(locale.DisplayLanguage);
                        break;
                    case LanguageAvailableResult.CountryAvailable:
                        langAvailable.Add(locale.DisplayLanguage);
                        break;
                    case LanguageAvailableResult.CountryVarAvailable:
                        langAvailable.Add(locale.DisplayLanguage);
                        break;
                }
            }
            langAvailable = langAvailable.OrderBy(t => t).Distinct().ToList();

            lang = Java.Util.Locale.Default;
            textToSpeech.SetLanguage(lang);

            // connect up the events
            btnSayIt.Click += delegate
            {
                // if there is nothing to say, don't say it
                var EmailsToRead = "";
                EmailsToRead += " Message is.: " + SMSMessage;
                EmailsToRead += " from.:" + SMSFrom;
                EmailsToRead += " sent on.: " + SMSDate ;

                if (!string.IsNullOrEmpty(EmailsToRead))
                {
                    if (EmailsToRead.Length > 3000)
                    {
                        textToSpeech.Speak(EmailsToRead.Substring(0, 3000), QueueMode.Add, null, null);

                    }
                    else
                    {
                        textToSpeech.Speak(EmailsToRead, QueueMode.Add, null, null);
                    }

                }
            };
            
        }

        private void GetSMSMessage(int id)
        {
            var filter = Telephony.Sms.Inbox.InterfaceConsts.Id + " = " + id.ToString();
            var cursor = Application.Context.ContentResolver.Query(Telephony.Sms.Inbox.ContentUri, null,
                filter, null, null);
            //Application.Context.ContentResolver.Query(Android.Net.Uri.Parse("content://sms/inbox"), null, null, null, null);

            if (cursor.MoveToFirst())
            {
                //Console.WriteLine("ID LIST:" + id);
                while (cursor.MoveToNext())
                {
                    //if (cursor.GetColumnIndex(Telephony.Sms.Inbox.InterfaceConsts.MessageTypeInbox)) { }
                    var ID = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.Inbox.InterfaceConsts.Id));
                    string From = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.Inbox.InterfaceConsts.Address)).ToString();
                    string messageVal = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.Inbox.InterfaceConsts.Body)).ToString();
                    DateTime Dateval = new DateTime(cursor.GetLong(cursor.GetColumnIndex(Telephony.Sms.Inbox.InterfaceConsts.Date)));
                    var currSMS = new MessageObject(ID, From, messageVal, Dateval);

                    
                }
            }
            cursor.Close();
        }

        private void GetSMSMessageByPosition(int id)
        {
            if (textToSpeech.IsSpeaking) { textToSpeech.Stop(); }

            var cursor = Application.Context.ContentResolver.Query(Telephony.Sms.Inbox.ContentUri, null,
                null, null, null);
            if (id <= maxPos - 1 && id >= 0)
            {
                if (id == maxPos - 1)
                {
                    var btnNext = FindViewById<Button>(Resource.Id.btnReadNextSMSReader);
                    btnNext.Enabled = false;
                }
                else
                {
                    var btnNext = FindViewById<Button>(Resource.Id.btnReadNextSMSReader);
                    btnNext.Enabled = true;
                }

                if (id >= 0)
                {
                    var btnPrev = FindViewById<Button>(Resource.Id.btnReadPrevSMSReader);
                    btnPrev.Enabled = true;
                }
                else
                {
                    var btnPrev = FindViewById<Button>(Resource.Id.btnReadPrevSMSReader);
                    btnPrev.Enabled = false;
                }

                if (cursor.MoveToPosition(id))
                {
                    var ID = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.Inbox.InterfaceConsts.Id));
                    string From = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.Inbox.InterfaceConsts.Address)).ToString();
                    string messageVal = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.Inbox.InterfaceConsts.Body)).ToString();

                    var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
                    var Dateval = epoch.AddMilliseconds(cursor.GetLong(cursor.GetColumnIndex(Telephony.Sms.Inbox.InterfaceConsts.Date))).AddHours(10);
                    

                    SMSFrom = From; // Intent.GetStringExtra("SMSFrom") ?? "From not available";
                    SMSDate = Dateval.ToString(); //Intent.GetStringExtra("SMSDate") ?? "Date not available";
                    SMSID = ID; //Intent.GetStringExtra("SMSID") ?? "ID not available";
                    SMSMessage = messageVal; //Intent.GetStringExtra("SMSMessage") ?? "Message not available";

                    curPos = id;

                    var tvFrom = FindViewById<TextView>(Resource.Id.tvFromSMSReader);
                    tvFrom.Text = SMSFrom;

                    var tvDate = FindViewById<TextView>(Resource.Id.tvDateSMSReader);
                    tvDate.Text = SMSDate;

                    var tvMessage = FindViewById<TextView>(Resource.Id.tvMessageSMSReader);
                    tvMessage.Text = SMSMessage;
                }
                cursor.Close();
            }
        }
        #endregion
    }
}