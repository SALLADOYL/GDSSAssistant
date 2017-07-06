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
using GDSSAssistant.Activities;
using GDSSAssistant.DataManager;
using System.Threading.Tasks;
using MailKit.Net.Pop3;
using MimeKit;
using System.Threading;

using SQLite;
using Android.Database.Sqlite;

namespace GDSSAssistant
{
    [Activity(Label = "GDSS ASSISTANT", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, TextToSpeech.IOnInitListener//, AudioManager.IOnAudioFocusChangeListener
    {

        private TextToSpeech textToSpeech;
        //Context context;
        private string ActivityStatus;
        private readonly int NeedLang = 103;
        private bool NeedToSpeak;
        private string MessageToSpeak;
        private Java.Util.Locale lang;

        //int count = 1;
        private bool isRecording;
        private readonly int VOICE = 10;
        private TextView STTtextBox;
        private ImageButton recButton;
        private ImageButton callButton;
        private Button btnSayIt;

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
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            using (DBManagerHelper dbmgr = new DBManagerHelper(this)) {
                dbmgr.OnCreate(null);
            }
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            
            // set the isRecording flag to false (not recording)
            isRecording = false;
            
            //initialize speech to text (input)
            InitSTTMethod();
            //initialize text to speech (read/speak)
            InitTTSMethod();

            callButton = FindViewById<ImageButton>(Resource.Id.imgBtnCall);
            callButton.Click += delegate {
                InvokeRead("Phone Call",0);
                StartActivity(typeof(Activities.PhoneCallActivity));
            };

            var smsButton = FindViewById<ImageButton>(Resource.Id.imgBtnSMS);
            smsButton.Click += delegate {
                InvokeRead("SMS Message", 0);
                StartActivity(typeof(Activities.MessageActivity));
            };

            var mailButton = FindViewById<ImageButton>(Resource.Id.imgBtnEmail);
            mailButton.Click += delegate {
                InvokeRead("Emails", 0);
                StartActivity(typeof(Activities.EmailActivity));
            };

            var otherButton = FindViewById<ImageButton>(Resource.Id.imgBtnOther);
            otherButton.Click += delegate {
                //StartActivity(typeof(Activities.EmailActivity));
            };


            var exitButton = FindViewById<ImageButton>(Resource.Id.imgBtnAppExit);
            exitButton.Click += delegate {
                //System.Environment.Exit(0);
                //this.FinishAffinity();
                Process.KillProcess(Process.MyPid());
            };

            ActivityStatus = "Created";
        }

        #region "Initialize Text-To-Speech"
        private void InitTTSMethod()
        {
            btnSayIt = FindViewById<Button>(Resource.Id.btnSpeak);
            var editWhatToSay = FindViewById<EditText>(Resource.Id.etxtTTS);

           
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

            //var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, langAvailable);
            //spinLanguages.Adapter = adapter;

            // set up the speech to use the default langauge
            // if a language is not available, then the default language is used.
            lang = Java.Util.Locale.Default;
            textToSpeech.SetLanguage(lang);

            // connect up the events
            btnSayIt.Click += delegate
            {
                // if there is nothing to say, don't say it
                if (!string.IsNullOrEmpty(editWhatToSay.Text))
                {
                    textToSpeech.Speak(editWhatToSay.Text, QueueMode.Flush, null,null);
                }
            };

        }
        #endregion
        
        #region "Initialize Speech to Text Library"
        private void InitSTTMethod()
        {

            //var tcs = new TaskCompletionSource<string>();

            // get the resources from the layout
            recButton = FindViewById<ImageButton>(Resource.Id.imgBtnListen);

            STTtextBox = FindViewById<TextView>(Resource.Id.etxtSTT);

            // check to see if we can actually record - if we can, assign the event to the button
            string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
            if (rec != "android.hardware.microphone")
            {
                // no microphone, no recording. Disable the button and output an alert
                var alert = new AlertDialog.Builder(recButton.Context);
                alert.SetTitle("You don't seem to have a microphone to record with");
                alert.SetPositiveButton("OK", (sender, e) =>
                {
                    STTtextBox.Text = "No microphone present";
                    Toast.MakeText(this, "No microphone present", 0);
                    recButton.Enabled = false;
                    return;
                });

                alert.Show();
            } 
             
                recButton.Click += delegate
                {
                    //isRecording = !isRecording;
                    if (textToSpeech.IsSpeaking)
                    {
                        textToSpeech.Stop();
                    }

                    if (!NeedToSpeak)
                    {
                        ListenToSpeech(0);
                    }
                    else {
                        InvokeRead();
                    };
                };
            
            //return tcs.Task;
        }
        #endregion

        protected override void OnStart()
        {
            base.OnStart();
            if (ActivityStatus == "Created") {
                ActivityStatus = "Start";
                //InvokeRead(GetString(Resource.String.CreatePrompt) + GetString(Resource.String.MainCommandList), 2000);
                SetReadMessage(GetString(Resource.String.CreatePrompt) + GetString(Resource.String.MainCommandList));
                //Console.WriteLine(Resource.String.CreatePrompt + GetString(Resource.String.MainCommandList));
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            //isRecording = !isRecording;
            if (textToSpeech.IsSpeaking)
            {
                textToSpeech.Stop();
            }

        }

        protected override void OnResume()
        {
            base.OnResume();
            if (ActivityStatus == "Created")
            {
                ActivityStatus = "Start";
                //InvokeRead(GetString(Resource.String.CreatePrompt) + GetString(Resource.String.MainCommandList), 2000);
                SetReadMessage(GetString(Resource.String.CreatePrompt) + GetString(Resource.String.MainCommandList));
                //Console.WriteLine(Resource.String.CreatePrompt + GetString(Resource.String.MainCommandList));
            }
            else {
                SetReadMessage(GetString(Resource.String.MainCommandList));
                    //"What's your command?");

            }
        }

        protected override void OnActivityResult(int req, Result res, Intent data)
        {
            var tvPreviewCommand = FindViewById<TextView>(Resource.Id.tvPreviewCommand);
            //STTtextBox = FindViewById<TextView>(Resource.Id.etxtCommand);
            if (req == NeedLang)
            {
                // we need a new language installed
                var installTTS = new Intent();
                installTTS.SetAction(TextToSpeech.Engine.ActionInstallTtsData);
                StartActivity(installTTS);

            } else if (req == VOICE)
            {
                if (res == Result.Ok)
                {
                    var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);

                    if (matches.Count != 0)
                    {
                        //string textInput = STTtextBox.Text + matches[0];
                        string textInput = matches[0];

                        // limit the output to 500 characters
                        if (textInput.Length > 500)
                            textInput = textInput.Substring(0, 500);

                        tvPreviewCommand.Text = textInput;
                        STTtextBox.Text = textInput;

                        //put logic transfer here. compare keywords then redirect to next activity.
                        if (textInput.ToLower() == "one" || textInput.ToLower() == "1" || textInput.ToLower() == "call" || textInput.ToLower() == "dial") {
                            //start activity here.
                            StartActivity(typeof(Activities.PhoneCallActivity));
                        } else if (textInput.ToLower() == "two" || textInput.ToLower() == "2" || textInput.ToLower() == "message" || textInput.ToLower() == "SMS") {
                            StartActivity(typeof(Activities.MessageActivity));
                        }
                        else if (textInput.ToLower() == "four" || textInput.ToLower() == "4" || textInput.ToLower() == "email" || textInput.ToLower() == "mail")
                        {
                            StartActivity(typeof(Activities.EmailActivity));
                        }
                        else if (textInput.ToLower() == "read email" || textInput.ToLower() == "5" || textInput.ToLower() == "five")
                        {

                            var progressDialog = ProgressDialog.Show(this, "Please wait...", "Connecting to GDSS Mail Server", true);
                            progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
                            new Thread(new ThreadStart(delegate
                            {
                                //LOAD METHOD TO GET ACCOUNT INFO                 
                                RunOnUiThread(() => {
                                    Toast.MakeText(this, "Email Sync: Connecting to Server", ToastLength.Long).Show();
                                });

                                RunOnUiThread(() => RetrievePOP3Mail());
                                //RetrievePOP3Mail();
                                RunOnUiThread(() => {
                                    Toast.MakeText(this, "Email Sync: Successful", ToastLength.Long).Show();
                                });
                                //RunOnUiThread(() => {
                                    DatabaseUpdates tblEmail = new DatabaseUpdates();
                                    List<EmailObject> EmailObjectList = tblEmail.EmailOBJ();
                                    int emailListCount = 0;
                                    EmailObject item = EmailObjectList[emailListCount];

                                    var actEmailReader = new Intent(this, typeof(EmailReaderActivity));
                                    actEmailReader.PutExtra("emlFrom", item.From);
                                    actEmailReader.PutExtra("emlCC", item.CC);
                                    actEmailReader.PutExtra("emlDate", item.date.ToString());
                                    actEmailReader.PutExtra("emlSubject", item.Subject);
                                    actEmailReader.PutExtra("emlBody", item.Body);
                                    actEmailReader.PutExtra("curPos", emailListCount);
                                    actEmailReader.PutExtra("maxPos", EmailObjectList.Count());
                                    actEmailReader.PutExtra("autoRead", 1);
                                    actEmailReader.SetFlags(ActivityFlags.ReorderToFront);
                                    StartActivity(actEmailReader);
                                //});

                                //HIDE PROGRESS DIALOG                 
                                RunOnUiThread(() => progressDialog.Hide());

                            })).Start();
                            
                        }
                        else if (textInput.ToLower() == "read message" || textInput.ToLower() == "read messages" || textInput.ToLower() == "3" || textInput.ToLower() == "three")
                        {
                            var progressDialog = ProgressDialog.Show(this, "Please wait...", "Fetching Messages", true);
                            progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
                            new Thread(new ThreadStart(delegate
                            {
                                //LOAD METHOD TO GET ACCOUNT INFO                 
                                RunOnUiThread(() => {
                                    Toast.MakeText(this, "SMS Sync: Fetching Messages", ToastLength.Long).Show();
                                });
                                
                                RunOnUiThread(() => {
                                    Toast.MakeText(this, "SMS Sync: Successful", ToastLength.Long).Show();
                                });
                                
                                //RunOnUiThread(() => {
                                var cursor = Application.Context.ContentResolver.Query(Telephony.Sms.Inbox.ContentUri, null, null, null, null);
                                var actSMSReader = new Intent(this, typeof(MessageReaderActivity));

                                cursor.MoveToFirst();

                                var ID = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.Inbox.InterfaceConsts.Id));
                                string From = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.Inbox.InterfaceConsts.Address)).ToString();
                                string messageVal = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.Inbox.InterfaceConsts.Body)).ToString();

                                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
                                var Dateval = epoch.AddMilliseconds(cursor.GetLong(cursor.GetColumnIndex(Telephony.Sms.Inbox.InterfaceConsts.Date))).AddHours(10);
                                var currSMS = new MessageObject(ID, From, messageVal, Dateval);
                                actSMSReader.PutExtra("SMSFrom", From);
                                actSMSReader.PutExtra("SMSID", ID);
                                actSMSReader.PutExtra("SMSDate", Dateval.ToString());
                                actSMSReader.PutExtra("SMSMessage", messageVal);
                                actSMSReader.PutExtra("curPos", 0);
                                actSMSReader.PutExtra("maxPos", cursor.Count - 1);
                                actSMSReader.PutExtra("autoRead", 1);
                                actSMSReader.SetFlags(ActivityFlags.ReorderToFront);
                                cursor.Close();
                                StartActivity(actSMSReader);
                                //});
                                //HIDE PROGRESS DIALOG                 
                                RunOnUiThread(() => progressDialog.Hide());
                            })).Start();

                            
                        }
                        else if (textInput.ToLower() == "six" || textInput.ToLower() == "6" || textInput.ToLower() == "assistant" || textInput.ToLower() == "google")
                        {

                            RunOnUiThread( async () =>
                            {
                                //Process.KillProcess(Process.MyPid());
                                var voiceAssistantIntent = new Intent(RecognizerIntent.ActionVoiceSearchHandsFree);
                                voiceAssistantIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);

                                // put a message on the modal dialog
                                voiceAssistantIntent.PutExtra(RecognizerIntent.ExtraPrompt, "Assistant Command");
                                //Application.Context.GetString(Resource.String));
                                    
                                // if there is more then 1.5s of silence, consider the speech over
                                voiceAssistantIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 3000);
                                voiceAssistantIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 3000);
                                voiceAssistantIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 3000);
                                voiceAssistantIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);

                                voiceAssistantIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
                                voiceAssistantIntent.SetFlags(ActivityFlags.ReorderToFront);
                                try {
                                    await Task.Delay(4000);
                                    StartActivity(voiceAssistantIntent);
                                } catch (Exception ex) {
                                    //Console.WriteLine(ex.Source + "|" + ex.Message); 
                                    Console.WriteLine(ex.Source + "|" + ex.Message);
                                }
                                
                            });

                        }
                        else if (textInput.ToLower() == "quit" || textInput.ToLower() == "exit" || textInput.ToLower() == "close")
                        {
                            Process.KillProcess(Process.MyPid());
                        }
                        else {
                            SetReadMessage("Unrecognized command. You said: " + textInput);
                            
                            var btnAction = FindViewById<ImageButton>(Resource.Id.imgBtnListen);
                            btnAction.SetImageResource(Resource.Drawable.micread);
                            //@android:drawable/presence_audio_online + Android.Resource.Drawable.PresenceAudioOnline + Android.Resource.Drawable.IcDialogInfo

                        }

                    } else {
                        STTtextBox.Text = "No speech was recognised";
                        SetReadMessage(STTtextBox.Text);
                    }
                }
            }

            base.OnActivityResult(req, res, data);
        }
        
        private void SetReadMessage(string ReadMessage) {
            NeedToSpeak = true;
            var etxtSpeak = FindViewById<EditText>(Resource.Id.etxtTTS);
            etxtSpeak.Text = ReadMessage;
            MessageToSpeak= ReadMessage;

            var btnAction = FindViewById<ImageButton>(Resource.Id.imgBtnListen);
            btnAction.SetImageResource(Resource.Drawable.micread);
        }

        private void RetrievePOP3Mail()
        {
            //string EmailsToRead = "";
            using (var client = new Pop3Client())
            {
                // accept all SSL certificates (in case the server supports STARTTLS)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                //Console.WriteLine("Connecting to MailServer" + GetString(Resource.String.GDSSEMailServerHost) +" port 995");
                Toast.MakeText(this, "Email Sync: Authenticating to Server", ToastLength.Long).Show();
                client.Connect(GetString(Resource.String.GDSSEMailServerHost), int.Parse(GetString(Resource.String.GDSSEMailServerPort)), true);

                // Note: since we don't have an OAuth2 token, disable
                // the XOAUTH2 authentication mechanism.
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(GetString(Resource.String.GDSSEMailAccount), GetString(Resource.String.GDSSEMailPwd));

                var emails = new List<EmailObject>();
                DBManagerHelper dbmgr = new DBManagerHelper(this);
                DatabaseUpdates tblEmail = new DatabaseUpdates();
                //var tblEmail = new EmailEntity();
                Toast.MakeText(this, "Email Sync: Downloading Emails", ToastLength.Long).Show();
                //for testing purpose only / clearing of table.
                if (int.Parse(GetString(Resource.String.TestDeleteEmailsOn)) == 1) { tblEmail.ClearMail(); }

                for (int i = client.Count - 1; i >= 0; i--)
                {
                    var message = new MimeMessage();

                    try
                    {
                        message = client.GetMessage(i);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Source + " || " + ex.Message);
                    }

                    if (!tblEmail.IsEmailExist(message.MessageId))
                    {
                        var curemail = new EmailObject(message.MessageId, message.From.ToString(), message.Subject, message.TextBody, message.Cc.ToString(), DateTime.Parse(message.Date.ToString()));

                        try
                        {
                            tblEmail.AddEmail(new EmailEntity
                            {
                                EmailID = curemail.EmailID ?? "0",
                                EmailFrom = curemail.From,
                                EmailCC = curemail.CC,
                                EmailDate = curemail.date,
                                EmailSubject = curemail.Subject,
                                EmailBody = curemail.Body

                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Source + "||" + ex.Message);
                        }
                    }
                }
                client.Disconnect(true);
                dbmgr.Close();
            }
        }

        private void InvokeRead() {
            if (NeedToSpeak)
            {
                var btnSpeak = FindViewById<Button>(Resource.Id.btnSpeak);
                btnSpeak.PerformClick();
                
                var btnAction = FindViewById<ImageButton>(Resource.Id.imgBtnListen);
                    btnAction.SetImageResource(Resource.Drawable.micspeak);
                    //@android:drawable/presence_audio_online + Android.Resource.Drawable.PresenceAudioOnline + Android.Resource.Drawable.IcDialogInfo

                    NeedToSpeak = false;
                }
        }
        
        private void InvokeRead(string TextToRead, long MillisecValue)
        {
            var editWhatToSay = FindViewById<EditText>(Resource.Id.etxtTTS);
            editWhatToSay.Text = TextToRead;
            NeedToSpeak = true;
            
            if (!string.IsNullOrEmpty(TextToRead))
            {

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

                var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, langAvailable);
                //spinLanguages.Adapter = adapter;

                // set up the speech to use the default langauge
                // if a language is not available, then the default language is used.
                lang = Java.Util.Locale.Default;
                textToSpeech.SetLanguage(lang);

                textToSpeech = new TextToSpeech(this, this, "com.google.android.tts");
                 textToSpeech.Speak(TextToRead, QueueMode.Flush, null,null);

                var btnAction = FindViewById<ImageButton>(Resource.Id.imgBtnListen);
                btnAction.SetImageResource(Resource.Drawable.micspeak);
                //@android:drawable/presence_audio_online + Android.Resource.Drawable.PresenceAudioOnline + Android.Resource.Drawable.IcDialogInfo
                
                NeedToSpeak = false;
                
            }

        }

        private void PerformPhoneCall(long MillisecValue)
        {
            string retListen = FindViewById<EditText>(Resource.Id.etxtSTT).Text ?? "";

            string CallReceiverCallNumber = SearchContactList_PhoneNumber(@"*120#");
            var uri = Android.Net.Uri.Parse(@"tel:" + CallReceiverCallNumber);
            var VoiceTriggeredCallIntent = new Intent(Intent.ActionCall, uri);
            StartActivity(VoiceTriggeredCallIntent);
        }

        private void ListenToSpeech(long MillisecValue)
        {// create the intent and start the activity
            var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);

            // put a message on the modal dialog
            voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, "GDSS Main Voice Recognition Input");
            //Application.Context.GetString(Resource.String));

            // if there is more then 1.5s of silence, consider the speech over
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 3000);
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 3000);
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 3000);
            voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);

            // you can specify other languages recognised here, for example
            // voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.German);
            // if you wish it to recognise the default Locale language and German

            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
            StartActivityForResult(voiceIntent, VOICE);


        }

        #region "Component Methods"
        private string SearchContactList_PhoneNumber(string Name) {

            var uri = ContactsContract.Contacts.ContentUri;
            var contactPhoneInfoUri = ContactsContract.CommonDataKinds.Phone.ContentUri;
            string contactID="";
            
            string[] projection = { ContactsContract.Contacts.InterfaceConsts.Id, ContactsContract.Contacts.InterfaceConsts.DisplayName};

            var cursor = Application.Context.ContentResolver.Query(ContactsContract.Contacts.ContentUri, null,
                ContactsContract.Contacts.InterfaceConsts.DisplayName + " = '" + Name + "'",null, null);
                //ManagedQuery(uri, projection, null, null, null);

            if (cursor.MoveToFirst())
            {
                bool contactfound = false;
                do
                {
                    //contactList.Add(cursor.GetString( cursor.GetColumnIndex(projection[1])));
                    if (cursor.GetString(cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName)).ToString().ToLower() == Name)
                    {
                        contactID= cursor.GetString(cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.Id));

                        // filtering phones related to a contact
                        var phones = Application.Context.ContentResolver.Query( ContactsContract.CommonDataKinds.Phone.ContentUri, null, 
                            ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId + " = " + contactID, null, null);

                        // getting phone numbers 
                        while (phones.MoveToNext()) {
                            var number = phones.GetString(//specify which column we want to get
                            phones.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.Number));
                            // do work with number
                            contactfound = true;
                            return number;
                        }
                        phones.Close();
                    }
                    //Console.WriteLine(cursor.GetString(
                    //    cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.Id)).ToString() + ":" + cursor.GetString(cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName)).ToString());
                } while (cursor.MoveToNext() || contactfound);
            }
            cursor.Close();
            return Name;
        }
        #endregion

    }


    //[Application]
    //public class BBSITCommAssitant : Application
    //{
    //    public override void OnCreate()
    //    {
    //        base.OnCreate();

    //        //This method needs to be called before any database calls can be made!
    //        DataManager.DBManagerHelper mydata = new DataManager.DBManagerHelper(this.ApplicationContext);

    //    }
    //}
}

