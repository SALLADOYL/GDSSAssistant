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
using Android.Provider;
using Android.Media;
using GDSSAssistant.DataManager;

namespace GDSSAssistant.Activities
{
    [Activity(Label = "GDSS: Email Reader")]
    public class EmailReaderActivity : Activity, TextToSpeech.IOnInitListener//, AudioManager.IOnAudioFocusChangeListener
    {
        private TextToSpeech textToSpeech;
        //Context context;
        private string ActivityStatus;
        private readonly int NeedLang = 103;
        //private bool NeedToSpeak;
        //private string MessageToSpeak;
        private Java.Util.Locale lang;

        //int count = 1;
        private bool isRecording;
        private readonly int VOICE = 10;
        //private TextView STTtextBox;
        //private ImageButton recButton;
        //private ImageButton callButton;
        private Button btnSayIt;
        private int curPos;
        private int maxPos;

        private string emlFrom;
        private string emlCC;
        private string emlDate;
        private string emlSubject;
        private string emlBody;

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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (textToSpeech.IsSpeaking)
            {
                textToSpeech.Stop();
            }
        }

        private void MoveMessageReaderByPos(int id) {
            if (textToSpeech.IsSpeaking) { textToSpeech.Stop(); }

            if (id <= maxPos - 1 && id >= 0)
            {
                if (id == maxPos - 1)
                {
                    var btnNext = FindViewById<Button>(Resource.Id.btnNextEmailReader);
                    btnNext.Enabled = false;
                }
                else
                {
                    var btnNext = FindViewById<Button>(Resource.Id.btnNextEmailReader);
                    btnNext.Enabled = true;
                }

                if (id >= 0)
                {
                    var btnPrev = FindViewById<Button>(Resource.Id.btnPrevEmailReader);
                    btnPrev.Enabled = true;
                }
                else
                {
                    var btnPrev = FindViewById<Button>(Resource.Id.btnPrevEmailReader);
                    btnPrev.Enabled = false;
                }

                //DBManagerHelper dbmgr = new DBManagerHelper(this);
                DatabaseUpdates tblEmail = new DatabaseUpdates();
                List<EmailObject> eListAdapt = tblEmail.EmailOBJ();
                EmailObject emlObj = eListAdapt[id];
                emlFrom = emlObj.From; //Intent.GetStringExtra("emlFrom") ?? "From Data not available";
                emlDate = emlObj.date.ToString(); //Intent.GetStringExtra("emlDate") ?? "Date Data not available";
                emlCC = emlObj.CC; //Intent.GetStringExtra("emlCC") ?? "CC Data not available";
                emlSubject = emlObj.Subject; //Intent.GetStringExtra("emlSubject") ?? "Subject Data not available";
                emlBody = emlObj.Body; //Intent.GetStringExtra("emlBody") ?? "Body Data not available";
                curPos = id; //Intent.GetIntExtra("curPos", 0);

                var tvFrom = FindViewById<TextView>(Resource.Id.tvFromEmailReaer);
                tvFrom.Text = emlFrom;
                var tvCC = FindViewById<TextView>(Resource.Id.tvCCEmailReader);
                tvCC.Text = emlCC;
                var tvSubject = FindViewById<TextView>(Resource.Id.tvSubjectEmailReader);
                tvSubject.Text = emlSubject;
                var tvBody = FindViewById<TextView>(Resource.Id.tvBodyEmailReader);
                tvBody.Text = emlBody;
            }
        }


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.EmailReader);

            // set the isRecording flag to false (not recording)
            isRecording = false;

            emlFrom = Intent.GetStringExtra("emlFrom") ?? "From Data not available";
            emlDate = Intent.GetStringExtra("emlDate") ?? "Date Data not available";
            emlCC = Intent.GetStringExtra("emlCC") ?? "CC Data not available";
            emlSubject = Intent.GetStringExtra("emlSubject") ?? "Subject Data not available";
            emlBody = Intent.GetStringExtra("emlBody") ?? "Body Data not available";
            curPos = Intent.GetIntExtra("curPos", 0);
            maxPos = Intent.GetIntExtra("maxPos", 0);
            var autoRead = Intent.GetIntExtra("autoRead", 0);

            var tvFrom = FindViewById<TextView>(Resource.Id.tvFromEmailReaer);
            tvFrom.Text = emlFrom;
            var tvCC  = FindViewById<TextView>(Resource.Id.tvCCEmailReader);
            tvCC.Text = emlCC;
            var tvSubject = FindViewById<TextView>(Resource.Id.tvSubjectEmailReader);
            tvSubject.Text  = emlSubject;
            var tvBody = FindViewById<TextView>(Resource.Id.tvBodyEmailReader);
            tvBody.Text = emlBody;

            //initialize text to speech (read/speak)
            InitTTSMethod();

            var backButton = FindViewById<Button>(Resource.Id.btnBackEmailReader);
            backButton.Click += delegate {
                Finish();
            };

            var btnStopSpeak = FindViewById<Button>(Resource.Id.btnStopEmailReader);
            btnStopSpeak.Click += delegate {
                if (textToSpeech.IsSpeaking)
                {
                    textToSpeech.Stop();
                }
            };

            var btnPrevEmailReader = FindViewById<Button>(Resource.Id.btnPrevEmailReader);
            btnPrevEmailReader.Click += delegate {
                MoveMessageReaderByPos(curPos - 1);
            };

            var btnNextEmailReader = FindViewById<Button>(Resource.Id.btnNextEmailReader);
            btnNextEmailReader.Click += delegate {
                MoveMessageReaderByPos(curPos + 1);
            };

            ActivityStatus = "Created";

            if (autoRead == 1)
            {
                var btnReadEmailReader = FindViewById<Button>(Resource.Id.btnReadEmailReader);
                btnReadEmailReader.PerformClick();
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
            btnSayIt = FindViewById<Button>(Resource.Id.btnReadEmailReader);

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
                //var curemail = new EmailObject(message.MessageId, message.From.ToString(), message.Subject, message.TextBody, message.Cc.ToString());
                EmailsToRead += " You have an email:" + emlSubject;
                EmailsToRead += " From: " + emlFrom;
                EmailsToRead += " Content is: " + emlBody;
                EmailsToRead += " On: " + emlDate;

                if (!string.IsNullOrEmpty(EmailsToRead))
                {
                    if (EmailsToRead.Length > 3000) {
                        textToSpeech.Speak(EmailsToRead.Substring(0,3000), QueueMode.Add, null, null);

                    } else {
                        textToSpeech.Speak(EmailsToRead, QueueMode.Add, null, null);
                    }
                    
                }
            };
        }
        #endregion


    }
}