using System;
using System.Text.RegularExpressions;
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
using MailKit.Net.Pop3;
using MimeKit;
using GDSSAssistant.DataManager;
using System.Threading;

namespace GDSSAssistant.Activities
{
    [Activity(Label = "GDSS: Email Assistant")]
    public class EmailActivity : Activity, TextToSpeech.IOnInitListener
    {
        private TextToSpeech textToSpeech;
        private string ActivityStatus;
        private readonly int NeedLang = 103;
        Java.Util.Locale lang;
        //private string StringSpeak;
        //private string StringListen;
        private string emailToValue;
        private string emailCCToValue;
        private string emailSubjectValue;
        private string emailBodyValue;
        private string emailConfirmSend;
        private bool NeedToSpeak;
        private string MessageToSpeak;
        private string EmailsToRead;
        private bool confirmFromRecipient;
        //private bool confirmCC;


        //int count = 1;
        private bool isRecording;
        private readonly int VOICE = 10;
        private TextView STTtextBox;
        private ImageButton recButton;
        //private Button btnSpeakIt;

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

        #region "Initialize Text-To-Speech"
        private void InitTTSMethod()
        {
            var btnSayIt = FindViewById<Button>(Resource.Id.btnSpeakEmailForm);
            var editWhatToSay = FindViewById<EditText>(Resource.Id.etxtTTSEmailForm);


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
                    
                    textToSpeech.Speak(editWhatToSay.Text, QueueMode.Flush, null, null);
                }
            };

        }
        private void SetReadMessage(string ReadMessage)
        {
            NeedToSpeak = true;
            var etxtSpeak = FindViewById<EditText>(Resource.Id.etxtTTSEmailForm);
            etxtSpeak.Text = ReadMessage;
            MessageToSpeak = ReadMessage;

            var btnAction = FindViewById<ImageButton>(Resource.Id.imgBtnListenEmailForm);
            btnAction.SetImageResource(Resource.Drawable.micread);
            //@android:drawable/presence_audio_online + Android.Resource.Drawable.PresenceAudioOnline + Android.Resource.Drawable.IcDialogInfo

        }

        
        private void InvokeRead()
        {
            if (NeedToSpeak)
            {
                var btnSpeak = FindViewById<Button>(Resource.Id.btnSpeakEmailForm);
                btnSpeak.PerformClick();

                var btnAction = FindViewById<ImageButton>(Resource.Id.imgBtnListenEmailForm);
                btnAction.SetImageResource(Resource.Drawable.micspeak);
                //@android:drawable/presence_audio_online + Android.Resource.Drawable.PresenceAudioOnline + Android.Resource.Drawable.IcDialogInfo
                NeedToSpeak = false;
            }
        }
        private void InvokeRead(string TextToRead, long MillisecValue)
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

            //var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, langAvailable);
            //spinLanguages.Adapter = adapter;

            // set up the speech to use the default langauge
            // if a language is not available, then the default language is used.
            lang = Java.Util.Locale.Default;
            textToSpeech.SetLanguage(lang);


            var editWhatToSay = FindViewById<EditText>(Resource.Id.etxtTTSEmailForm);
            editWhatToSay.Text = TextToRead;

            if (!string.IsNullOrEmpty(TextToRead))
            {
                //new Thread(new ThreadStart(() =>
                //{
                //    RunOnUiThread(() =>
                //    {
                        textToSpeech = new TextToSpeech(this, this, "com.google.android.tts");
                        textToSpeech.Speak(TextToRead, QueueMode.Flush, null, null);
                //    });
                //})).Start();
            }

            var btnAction = FindViewById<ImageButton>(Resource.Id.imgBtnListenEmailForm);
            btnAction.SetImageResource(Resource.Drawable.micspeak);
            //@android:drawable/presence_audio_online + Android.Resource.Drawable.PresenceAudioOnline + Android.Resource.Drawable.IcDialogInfo

            NeedToSpeak = false;
        }

        #endregion

        #region "Initialize Speech to Text Library"
        private void InitSTTMethod()
        {

            var btnSayIt = FindViewById<Button>(Resource.Id.btnSpeakEmailForm);

            // get the resources from the layout
            recButton = FindViewById<ImageButton>(Resource.Id.imgBtnListenEmailForm);

            STTtextBox = FindViewById<TextView>(Resource.Id.etxtTTSEmailForm);

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
                    recButton.Enabled = false;
                    return;
                });

                alert.Show();
            }

            recButton.Click += delegate
            {
                if (NeedToSpeak)
                {
                    InvokeRead();
                }
                else
                {
                    if (AllEmailInputsValid() && emailConfirmSend == "yes")
                    {

                        //SendSMS(smsTOValue, smsBodyValue);
                        SendEmail(emailToValue, emailCCToValue, emailSubjectValue, emailBodyValue);
                    }
                    else
                    {
                        ListenToSpeech(0);
                    }
                }
                //}
            };

            //return tcs.Task;
        }

        private bool AllEmailInputsValid()
        {
            if (!string.IsNullOrEmpty(emailToValue)
                && !string.IsNullOrEmpty(emailSubjectValue)
                && !string.IsNullOrEmpty(emailBodyValue)
                )
            { return true; }
            else { return false; }
        }

        private void ListenToSpeech(long MillisecValue)
        {// create the intent and start the activity
            var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);

            // put a message on the modal dialog
            voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, "GDSS Email Voice Input");
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

            var btnAction = FindViewById<ImageButton>(Resource.Id.imgBtnListenEmailForm);
            btnAction.SetImageResource(Resource.Drawable.micread);
            //@android:drawable/presence_audio_online + Android.Resource.Drawable.PresenceAudioOnline + Android.Resource.Drawable.IcDialogInfo

        }
        #endregion

        protected override void OnActivityResult(int req, Result res, Intent data)
        {

            if (req == NeedLang)
            {
                // we need a new language installed
                var installTTS = new Intent();
                installTTS.SetAction(TextToSpeech.Engine.ActionInstallTtsData);
                StartActivity(installTTS);
            }
            else if (req == VOICE)
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

                        if (textInput.ToLower() == "cancel" || textInput.ToLower() == "reset")
                        {
                            Finish();
                            StartActivity(typeof(EmailActivity));
                        }

                        //put logic transfer here. compare keywords then redirect to next activity.
                        //if smsTOtxt = 0, set value
                        if (string.IsNullOrEmpty(emailToValue) || !confirmFromRecipient)
                        {
                            if (!confirmFromRecipient && textInput.ToLower() == "no")
                            {
                                confirmFromRecipient = true;

                                var etxtEmailTo = FindViewById<TextView>(Resource.Id.etxtEmailTo);
                                etxtEmailTo.Text = emailToValue;

                                SetReadMessage("recipient.: " + emailToValue.Replace(";", " and. ") + ", what is the Subject?");
                            } else if (!confirmFromRecipient && textInput.ToLower() == "yes")
                            {
                                confirmFromRecipient = false;
                                SetReadMessage("Who else is the recipient of the email?");
                            } else {
                                confirmFromRecipient = false;
                                var strContact = SearchContactList_EmailAddress(textInput);
                                if (string.IsNullOrEmpty(emailToValue))
                                {
                                    emailToValue = strContact;
                                } else {
                                    emailToValue += " ; " + strContact;
                                }

                                //STTtextBox = FindViewById<EditText>(Resource.Id.etxtEmailTo);
                                //STTtextBox.Text = strContact;
                                var etxtEmailTo = FindViewById<TextView>(Resource.Id.etxtEmailTo);
                                etxtEmailTo.Text = emailToValue;
                                SetReadMessage("recipient.: " + emailToValue.Replace(";", " and. ") + ", would you like to add another recipient?");
                            }

                        }
                        else if (string.IsNullOrEmpty(emailSubjectValue))
                        {//if smsBodyValue =0, set value,
                            emailSubjectValue = textInput;
                            //STTtextBox = FindViewById<EditText>(Resource.Id.etxtEmailSubject);
                            //STTtextBox.Text = textInput;
                            //Console.WriteLine("MessageActivity: emailSubjectValue=" + textInput);
                            SetReadMessage("The subject is:" + textInput + ", what is the message?");
                        }
                        else if (string.IsNullOrEmpty(emailBodyValue))
                        {//if smsBodyValue =0, set value,
                            emailBodyValue = textInput;
                            //STTtextBox = FindViewById<EditText>(Resource.Id.etxtEmailBody);
                            //STTtextBox.Text = textInput;
                            //Console.WriteLine("MessageActivity: smsBodytxt=" + textInput);
                            SetReadMessage("Your message is:" + textInput + ",do you want to send this message?");
                        }
                        else if (string.IsNullOrEmpty(emailConfirmSend))
                        {
                            if (textInput.ToLower() == "yes")
                            {
                                emailConfirmSend = textInput.ToLower();
                                //STTtextBox = FindViewById<EditText>(Resource.Id.etxtTTSEmailForm);
                                //STTtextBox.Text = textInput;
                            }
                            else
                            {
                                SetReadMessage("Your message is:" + textInput + ",do you want to send this message?");
                            }
                        }

                        if (AllEmailInputsValid() && emailConfirmSend == "yes")
                        {
                            ActivityStatus = "Sending a Message";

                            SendEmail(emailToValue, emailCCToValue, emailSubjectValue, emailBodyValue);
                            emailToValue = "";
                            emailCCToValue = "";
                            emailSubjectValue = "";
                            emailBodyValue = "";
                        }
                    }
                    else
                    {
                        SetReadMessage("Unrecognized Command");
                    }
                }
            }
            base.OnActivityResult(req, res, data);
        }


        protected override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.EmailForm);
                // Create your application here

                isRecording = false;

                //initialize speech to text (input)
                InitSTTMethod();
                //initialize text to speech (read/speak)
                InitTTSMethod();

                var backButton = FindViewById<Button>(Resource.Id.btnBackEmailForm);
                backButton.Click += delegate { Finish(); };


                var btnGetEmails = FindViewById<Button>(Resource.Id.btnGetMailPOP3EmailForm);
                btnGetEmails.Click += delegate
                {
                    var progressDialog = ProgressDialog.Show(this, "Please wait...", "Connecting to GDSS Mail Server", true);
                    progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
                    new Thread(new ThreadStart(delegate
                    {
                        //LOAD METHOD TO GET ACCOUNT INFO                 
                        RunOnUiThread(() => {
                            Toast.MakeText(this, "Email Sync: Connecting to Server", ToastLength.Long).Show();
                        });
                        //RetrievePOP3Mail();
                        RunOnUiThread(() => RetrievePOP3Mail());
                        RunOnUiThread(() => {
                            Toast.MakeText(this, "Email Sync: Successful", ToastLength.Long).Show();
                        });
                        RunOnUiThread(() => RetrieveDBMail());
                    //HIDE PROGRESS DIALOG                 
                    RunOnUiThread(() => progressDialog.Hide());
                    })).Start();
                };

                var btnResetEmailForm = FindViewById<Button>(Resource.Id.btnResetEmailForm);
                btnResetEmailForm.Click += delegate
                {
                    Finish();
                    StartActivity(typeof(EmailActivity));
                };

            var lvEmail = FindViewById<ListView>(Resource.Id.lvEmailList);
            lvEmail.ItemClick += (sender, e) => {
                EmailListAdapter EmailList = ((EmailListAdapter)lvEmail.Adapter);
                EmailObject item = EmailList[e.Position];
                
                var actEmalReader = new Intent(this, typeof(EmailReaderActivity));
                actEmalReader.PutExtra("emlFrom", item.From);
                actEmalReader.PutExtra("emlCC", item.CC);
                actEmalReader.PutExtra("emlDate", item.date.ToString());
                actEmalReader.PutExtra("emlSubject", item.Subject);
                actEmalReader.PutExtra("emlBody", item.Body);
                actEmalReader.PutExtra("curPos", e.Position);
                actEmalReader.PutExtra("maxPos", EmailList.Count);
                actEmalReader.PutExtra("autoRead", 1);
                actEmalReader.SetFlags(ActivityFlags.ReorderToFront);
                StartActivity(actEmalReader);
            };

            ActivityStatus = "Created";

            RetrieveDBMail();

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
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
                SetReadMessage(GetString(Resource.String.EmailAskWhoToEmail));
                //Console.WriteLine("Email Activity:" + Resource.String.EmailAskWhoToEmail);
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            // always call the base implementation!
            base.OnSaveInstanceState(outState);
        }
        protected override void OnStart()
        {
            base.OnStart();
            if (ActivityStatus == "Created")
            {
                ActivityStatus = "Start";
                SetReadMessage(GetString(Resource.String.EmailAskWhoToEmail));
            }
        }

        #region "Component Methods"

        private string SearchContactList_EmailAddress(string Name)
        {

            var uri = ContactsContract.Contacts.ContentUri;
            var contactPhoneInfoUri = ContactsContract.CommonDataKinds.Phone.ContentUri;
            string contactID = "";

            string[] projection = { ContactsContract.Contacts.InterfaceConsts.Id, ContactsContract.Contacts.InterfaceConsts.DisplayName };

            var cursor = Application.Context.ContentResolver.Query(ContactsContract.Contacts.ContentUri, null,
                ContactsContract.Contacts.InterfaceConsts.DisplayName + " like '%" + Name.ToLower() + "%'", null, null);
            //ManagedQuery(uri, projection, null, null, null);

            if (cursor.MoveToFirst())
            {
                bool contactfound = false;
                do
                {
                    //contactList.Add(cursor.GetString( cursor.GetColumnIndex(projection[1])));
                    if (cursor.GetString(cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName)).ToString().ToLower() == Name.ToLower())
                    {
                        contactID = cursor.GetString(cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.Id));

                        // filtering phones related to a contact
                        var emails = Application.Context.ContentResolver.Query(ContactsContract.CommonDataKinds.Email.ContentUri, null,
                            ContactsContract.CommonDataKinds.Email.InterfaceConsts.ContactId + " = " + contactID, null, null);

                        // getting phone numbers 
                        while (emails.MoveToNext())
                        {
                            var emailaddress = emails.GetString(//specify which column we want to get
                            emails.GetColumnIndex(ContactsContract.CommonDataKinds.Email.Address));
                            // do work with number
                            contactfound = true;
                            return emailaddress;
                        }
                        emails.Close();
                    }
                    //Console.WriteLine(cursor.GetString(
                    //    cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.Id)).ToString() + ":" + cursor.GetString(cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName)).ToString());
                } while (cursor.MoveToNext() || contactfound);
            }
            cursor.Close();
            return Name;
        }
        private void SendEmail(string mailTo,string mailCC, string mailSubj, string mailBody )
        {
            //var mailTo = "";// FindViewById<EditText>(Resource.Id.etxtEmailTo);
            //var mailCC = "";//FindViewById<EditText>(Resource.Id.etxtEmailCC);
           // var mailSubj = "";//FindViewById<EditText>(Resource.Id.etxtEmailSubject);
            //var mailBody = "";// FindViewById<EditText>(Resource.Id.etxtEmailBody);

            //var smsUri = Android.Net.Uri.Parse("smsto:" + smsTOtxt.Text);
            //var smsIntent = new Intent(Intent.ActionSendto, smsUri);
            //smsIntent.PutExtra("sms_body", smsBodytxt.Text);
            //StartActivity(smsIntent);

            var email = new Intent(Android.Content.Intent.ActionSend);
            string[] recipient = mailTo.Split(';');
            email.PutExtra(Android.Content.Intent.ExtraEmail, recipient);
                //new string[] { mailTo } );
            email.PutExtra(Android.Content.Intent.ExtraCc, mailCC);
            //email.PutExtra(Android.Content.Intent.ExtraBcc, new string[] { "person3@xamarin.com" });
            email.PutExtra(Android.Content.Intent.ExtraSubject, mailSubj);
            email.PutExtra(Android.Content.Intent.ExtraText, mailBody);
            
            email.SetType("message/rfc822");
            //StartActivity(Intent.CreateChooser(email, "Send Email"));
            StartActivity(email);
        }

        private void RetrievePOP3Mail() {
            EmailsToRead = "";
            using (var client = new Pop3Client())
            {
                // accept all SSL certificates (in case the server supports STARTTLS)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                //Console.WriteLine("Connecting to MailServer" + GetString(Resource.String.GDSSEMailServerHost) +" port 995");
                //Toast.MakeText(this, "Email Sync: Authenticating to Server", ToastLength.Long).Show();
                client.Connect(GetString(Resource.String.GDSSEMailServerHost), int.Parse(GetString(Resource.String.GDSSEMailServerPort)), true);

                // Note: since we don't have an OAuth2 token, disable
                // the XOAUTH2 authentication mechanism.
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(GetString(Resource.String.GDSSEMailAccount), GetString(Resource.String.GDSSEMailPwd));

                var emails = new List<EmailObject>();
                DBManagerHelper dbmgr = new DBManagerHelper(this);
                DatabaseUpdates tblEmail = new DatabaseUpdates();
                //var tblEmail = new EmailEntity();
                //Toast.MakeText(this, "Email Sync: Downloading Emails", ToastLength.Long).Show();
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

        private void RetrieveDBMail()
        {
            EmailsToRead = "";
            DBManagerHelper dbmgr = new DBManagerHelper(this);
            DatabaseUpdates tblEmail = new DatabaseUpdates();
            var eListAdapt = new EmailListAdapter(this, tblEmail.EmailOBJ());
                eListAdapt.NotifyDataSetChanged();

                var emailListView = FindViewById<ListView>(Resource.Id.lvEmailList);
                emailListView.Adapter = eListAdapt;
        }

        #endregion
    }

    public class EmailObject {
        public string EmailID;
        public string From;
        public string CC;
        public string Subject;
        public string Body;
        public DateTime date;
        public EmailObject(string EmailID, string From, string Subject,string Body, string CC, DateTime date1) {
            this.EmailID = EmailID;
            this.From = From;
            this.Subject = Subject;
            this.Body = Body;
            this.CC = CC;
            this.date = date1;
        }

    }
    public class EmailListAdapter : BaseAdapter<EmailObject> {
         List<EmailObject> items;
        Activity context;
        public EmailListAdapter(Activity context, List<EmailObject> items) : base()
        {
            this.context = context;
            this.items = items;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override EmailObject this[int position]
        {
            get { return items[position]; }
        }

        public override int Count
        {
            get {
                if (items == null)
                {
                    return 0;
                }
                else
                {
                    return items.Count() - 1;
                }
            }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];
            View view = convertView;
            if (view == null) // no view to re-use, create new
                view = context.LayoutInflater.Inflate(Resource.Layout.EmailList, null);
                var etxtEmailFromList = view.FindViewById<TextView>(Resource.Id.etxtEmailFromList);
                etxtEmailFromList.Text  = item.From;
            
                var etxtEmailSubjectList = view.FindViewById<TextView>(Resource.Id.etxtEmailSubjectList);
                etxtEmailSubjectList.Text = item.Subject;
            
            return view;
        }
    }
}