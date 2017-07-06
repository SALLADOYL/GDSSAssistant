using System;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Speech.Tts;
using Android.Speech;
using System.Collections.Generic;
using System.Linq;
using Android.Provider;
using System.Threading;


namespace GDSSAssistant.Activities
{
    [Activity(Label = "GDSS: SMS Assistant")]
    public class MessageActivity : Activity, TextToSpeech.IOnInitListener
    {
        private TextToSpeech textToSpeech;
        private string ActivityStatus;
        private readonly int  NeedLang = 103;
        private Java.Util.Locale lang;
        private string smsTOValue;
        private string smsBodyValue;
        private string smsConfirmSend;
        private bool NeedToSpeak;
        private string MessageToSpeak;

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
            var btnSayIt = FindViewById<Button>(Resource.Id.btnSpeakSMSForm);
            var editWhatToSay = FindViewById<EditText>(Resource.Id.etxtTTSSMSForm);


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
                if (!string.IsNullOrEmpty(MessageToSpeak))
                {
                    textToSpeech.Speak(editWhatToSay.Text, QueueMode.Flush, null,null);
                }
            };

        }
        private void SetReadMessage(string ReadMessage)
        {
            NeedToSpeak = true;
            var etxtSpeak = FindViewById<EditText>(Resource.Id.etxtTTSSMSForm);
            etxtSpeak.Text = ReadMessage;
            MessageToSpeak = ReadMessage;

            var btnAction = FindViewById<ImageButton>(Resource.Id.imgBtnListenSMSForm);
            btnAction.SetImageResource(Resource.Drawable.micread);
            //@android:drawable/presence_audio_online + Android.Resource.Drawable.PresenceAudioOnline + Android.Resource.Drawable.IcDialogInfo

        }
        private void InvokeRead()
        {
            if (NeedToSpeak)
            {
                var btnSpeak = FindViewById<Button>(Resource.Id.btnSpeakSMSForm);
                btnSpeak.PerformClick();

                var btnAction = FindViewById<ImageButton>(Resource.Id.imgBtnListenSMSForm);
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


            var editWhatToSay = FindViewById<EditText>(Resource.Id.etxtTTSSMSForm);
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

            var btnAction = FindViewById<ImageButton>(Resource.Id.imgBtnListenSMSForm);
            btnAction.SetImageResource(Resource.Drawable.micspeak);
            //@android:drawable/presence_audio_online + Android.Resource.Drawable.PresenceAudioOnline + Android.Resource.Drawable.IcDialogInfo

            NeedToSpeak = false;
        }

        #endregion

        #region "Initialize Speech to Text Library"
        private void InitSTTMethod()
        {

            var btnSayIt = FindViewById<Button>(Resource.Id.btnSpeakSMSForm);

            // get the resources from the layout
            recButton = FindViewById<ImageButton>(Resource.Id.imgBtnListenSMSForm);

            STTtextBox = FindViewById<TextView>(Resource.Id.etxtSMSToSMSForm);

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
                    if (AllSMSInputsValid() && smsConfirmSend=="yes")
                    {
                        //PerformPhoneCall(0);
                        SendSMS(smsTOValue, smsBodyValue);
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

        private bool AllSMSInputsValid()
        {
            if (!string.IsNullOrEmpty(smsTOValue) && !string.IsNullOrEmpty(smsBodyValue))
            { return true; }
            else { return false; }
        }

        private void ListenToSpeech(long MillisecValue)
        {// create the intent and start the activity
            var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);

            // put a message on the modal dialog
            voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, "GDSS SMS Voice Input");
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

            var btnAction = FindViewById<ImageButton>(Resource.Id.imgBtnListenSMSForm);
            btnAction.SetImageResource(Resource.Drawable.micread);
            //@android:drawable/presence_audio_online + Android.Resource.Drawable.PresenceAudioOnline + Android.Resource.Drawable.IcDialogInfo

        }
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.MessageForm);
            isRecording = false;

            //initialize speech to text (input)
            InitSTTMethod();
            //initialize text to speech (read/speak)
            InitTTSMethod();

            var backButton = FindViewById<Button>(Resource.Id.btnBackSMSForm);
            backButton.Click += delegate { Finish(); };

            var btnGetMessages = FindViewById<Button>(Resource.Id.btnGetSMSMessageForm);
            btnGetMessages.Click += delegate
            {
                var progressDialog = ProgressDialog.Show(this, "Please wait...", "Fetching Messages", true);
                progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
                new Thread(new ThreadStart(delegate
                {
                    //LOAD METHOD TO GET ACCOUNT INFO                 
                    RunOnUiThread(() => {
                        Toast.MakeText(this, "SMS Sync: Fetching Messages", ToastLength.Long).Show();
                    });
                    RunOnUiThread(() => GetSMSMessages());
                    //GetSMSMessages();
                    RunOnUiThread(() => {
                        Toast.MakeText(this, "SMS Sync: Successful", ToastLength.Long).Show();
                    });
                   
                    //HIDE PROGRESS DIALOG                 
                    RunOnUiThread(() => progressDialog.Hide());
                })).Start();
                
            };

            var btnResetSMSMessageForm = FindViewById<Button>(Resource.Id.btnResetSMSMessageForm);
            btnResetSMSMessageForm.Click += delegate
            {
                smsTOValue = "";
                smsBodyValue = "";
                smsConfirmSend = "";
                NeedToSpeak = true;
                //this.OnCreate(null);
                Finish();
                StartActivity(typeof(MessageActivity));
            };

            var lvSMS = FindViewById<ListView>(Resource.Id.lvSMSList);
            lvSMS.ItemClick += (sender, e) => {
                SMSListAdapter SMSList = ((SMSListAdapter)lvSMS.Adapter);
                MessageObject item = SMSList[e.Position];

                var actSMSReader = new Intent(this , typeof(MessageReaderActivity));
                actSMSReader.PutExtra("SMSFrom", item.From);
                actSMSReader.PutExtra("SMSID", item.MessageID);
                actSMSReader.PutExtra("SMSDate", item.Date.ToString());
                actSMSReader.PutExtra("SMSMessage", item.Message);
                actSMSReader.PutExtra("curPos", e.Position);
                actSMSReader.PutExtra("maxPos", ((SMSListAdapter)lvSMS.Adapter).Count);
                actSMSReader.PutExtra("autoRead", 1);
                actSMSReader.SetFlags(ActivityFlags.ReorderToFront);
                StartActivity(actSMSReader);
            };

            ActivityStatus = "Created";

            GetSMSMessages();

        }

        protected override void OnResume()
        {
            base.OnResume();
            if (ActivityStatus == "Created")
            {
                ActivityStatus = "Start";
                SetReadMessage(GetString(Resource.String.SMSAskWhoToMessage));
                //Console.WriteLine("Message Activity:" + Resource.String.SMSAskWhoToMessage);
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            //outState.PutInt("click_count", _counter);
            //Log.Debug(GetType().FullName, "Activity A - Saving instance state");

            // always call the base implementation!
            base.OnSaveInstanceState(outState);
        }
        protected override void OnStart()
        {
            base.OnStart();
            if (ActivityStatus == "Created")
            {
                ActivityStatus = "Start";
                //InvokeRead(GetString(Resource.String.SMSAskWhoToMessage), 0);
                SetReadMessage(GetString(Resource.String.SMSAskWhoToMessage));
                //Console.WriteLine("Message Activity:" + Resource.String.SMSAskWhoToMessage);
            }
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
                            StartActivity(typeof(PhoneCallActivity));
                        }

                        //put logic transfer here. compare keywords then redirect to next activity.
                        //if smsTOtxt = 0, set value
                        if (string.IsNullOrEmpty(smsTOValue))
                        {
                            var strContact = SearchContactList_PhoneNumber(textInput);
                            smsTOValue = strContact;
                            STTtextBox = FindViewById<EditText>(Resource.Id.etxtSMSToSMSForm);
                            STTtextBox.Text = strContact;

                            string strCheckPhone = SearchContactList_PhoneNumber(textInput);

                            if (strCheckPhone == textInput)
                            {
                                //SetReadMessage("Contact " + textInput + " not found. Press the button to call anyway");
                                SetReadMessage(textInput + " is not in your contact list, what is your message?");
                            }
                            else
                            {
                                SetReadMessage("The recipient is: " + textInput + "., what is your message?");
                            }
                        }
                        else if (string.IsNullOrEmpty(smsBodyValue))
                        {
                            smsBodyValue = textInput;
                            STTtextBox = FindViewById<EditText>(Resource.Id.etxtSMSBodySMSForm);
                            STTtextBox.Text = textInput;
                            SetReadMessage("Your message is:" + textInput + ". do you want to send this message?");
                        }
                        else if (string.IsNullOrEmpty(smsConfirmSend))
                        {
                            if (textInput.ToLower() == "yes")
                            {
                                smsConfirmSend = textInput.ToLower();
                                STTtextBox = FindViewById<EditText>(Resource.Id.etxtSMSBodySMSForm);
                                STTtextBox.Text = textInput;
                            }
                            else {
                                SetReadMessage("Your message is:" + textInput + ". do you want to send this message?");
                            }
                            
                        }

                        if (AllSMSInputsValid() && smsConfirmSend == "yes")
                        {
                            ActivityStatus = "Sending a Message";
                            //Console.WriteLine("MessageActivity: " + ActivityStatus);

                            SendSMS(smsTOValue, smsBodyValue);
                            smsBodyValue = "";
                            smsTOValue = "";
                        }
                        else {
                            smsTOValue = "";
                            smsBodyValue = "";
                            smsConfirmSend = "";
                            NeedToSpeak = true;
                            //this.OnCreate(null);
                            Finish();
                            StartActivity(typeof(MessageActivity));
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

        #region "Component Methods"
        private string SearchContactList_PhoneNumber(string Name)
        {

            var uri = ContactsContract.Contacts.ContentUri;
            var contactPhoneInfoUri = ContactsContract.CommonDataKinds.Phone.ContentUri;
            string contactID = "";

            string[] projection = { ContactsContract.Contacts.InterfaceConsts.Id, ContactsContract.Contacts.InterfaceConsts.DisplayName };

            var cursor = Application.Context.ContentResolver.Query(ContactsContract.Contacts.ContentUri, null,
                ContactsContract.Contacts.InterfaceConsts.DisplayName + " = '" + Name + "'", null, null);
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
                        var phones = Application.Context.ContentResolver.Query(ContactsContract.CommonDataKinds.Phone.ContentUri, null,
                            ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId + " = " + contactID, null, null);

                        // getting phone numbers 
                        while (phones.MoveToNext())
                        {
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
        //private string SearchContactList_DisplayName(string Address)
        //{

        //    // filtering phones related to a contact according to address
        //    var phones = Application.Context.ContentResolver.Query(ContactsContract.CommonDataKinds.Phone.ContentUri, null,
        //        ContactsContract.CommonDataKinds.Phone.InterfaceConsts.n + " = " + Address, null, null);

        //    var uri = ContactsContract.Contacts.ContentUri;
        //    var contactPhoneInfoUri = ContactsContract.CommonDataKinds.Phone.ContentUri;
        //    string contactID = "";

        //    string[] projection = { ContactsContract.Contacts.InterfaceConsts.Id, ContactsContract.Contacts.InterfaceConsts.DisplayName };

        //    var cursor = Application.Context.ContentResolver.Query(ContactsContract.Contacts.ContentUri, null,
        //        ContactsContract.CommonDataKinds.Phone.Number + " = '" + Address + "'", null, null);
        //    //ManagedQuery(uri, projection, null, null, null);

        //    if (cursor.MoveToFirst())
        //    {
        //        bool contactfound = false;
        //        do
        //        {
        //            //contactList.Add(cursor.GetString( cursor.GetColumnIndex(projection[1])));
        //            if (cursor.GetString(cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName)).ToString().ToLower() == Name.ToLower())
        //            {
        //                contactID = cursor.GetString(cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.Id));

        //                // filtering phones related to a contact
        //                var phones = Application.Context.ContentResolver.Query(ContactsContract.CommonDataKinds.Phone.ContentUri, null,
        //                    ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId + " = " + contactID, null, null);

        //                // getting phone numbers 
        //                while (phones.MoveToNext())
        //                {
        //                    var number = phones.GetString(//specify which column we want to get
        //                    phones.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.Number));

        //                    //phones.
        //                    // do work with number
        //                    contactfound = true;
        //                    return number;
        //                }
        //                phones.Close();
        //            }
        //            Console.WriteLine(cursor.GetString(
        //                cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.Id)).ToString() + ":" + cursor.GetString(cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName)).ToString());
        //        } while (cursor.MoveToNext() || contactfound);
        //    }
        //    cursor.Close();
        //    return Name;
        //}
        public static String getContactName(String phoneNumber)
        {
            //ContentResolver cr = context.getContentResolver();
            //Uri uri = Uri(PhoneLookup.CONTENT_FILTER_URI, Uri.encode(phoneNumber));
            string[] projection = { ContactsContract.PhoneLookup.InterfaceConsts.NormalizedNumber, ContactsContract.PhoneLookup.InterfaceConsts.DisplayName };
            var cursor = Application.Context.ContentResolver.Query(ContactsContract.PhoneLookup.ContentFilterUri, projection,
                ContactsContract.PhoneLookup.InterfaceConsts.NormalizedNumber + " = '" + phoneNumber + "'", null, null);

            if (cursor == null)
            {
                return phoneNumber;
            }

            String contactName = null;

            if (cursor.MoveToFirst())
            {
                contactName = cursor.GetString(cursor.GetColumnIndex(ContactsContract.PhoneLookup.InterfaceConsts.DisplayName));
            }

            if (cursor != null && !cursor.IsClosed)
            {
                cursor.Close();
            }

            return contactName;
        }
        private void SendSMS(string smsTO, string smsBody)
        {
                var smsUri = Android.Net.Uri.Parse("smsto:" + smsTO);
                var smsIntent = new Intent(Intent.ActionSendto, smsUri);
                smsIntent.PutExtra("sms_body", smsBody);

                StartActivity(smsIntent);

            smsTOValue = "";
            smsBodyValue = "";

            //SetReadMessage("Press button and say the Name or the Number of the contact you would like to call.");

        }

        private void GetSMSMessages() {
            var cursor = Application.Context.ContentResolver.Query(Telephony.Sms.Inbox.ContentUri, null, null, null, null);
            //Application.Context.ContentResolver.Query(Android.Net.Uri.Parse("content://sms/inbox"), null, null, null, null);

            var SMS = new List<MessageObject>();

            if (cursor.MoveToFirst())
            {
                //Console.WriteLine("ID LIST:");
                do
                {
                    var ID = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.Inbox.InterfaceConsts.Id));
                    string From = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.Inbox.InterfaceConsts.Address)).ToString();
                    //string From = getContactName(cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.Inbox.InterfaceConsts.Address)).ToString());
                    //string Creator = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.Inbox.InterfaceConsts.Address)).ToString();
                    string messageVal = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.Inbox.InterfaceConsts.Body)).ToString();

                    var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
                    var Dateval = epoch.AddMilliseconds(cursor.GetLong(cursor.GetColumnIndex(Telephony.Sms.Inbox.InterfaceConsts.Date))).AddHours(10);
                    var currSMS = new MessageObject(ID, From, messageVal, Dateval);
                    SMS.Add(currSMS);
                } while (cursor.MoveToNext());
            }
            cursor.Close();

            var eListAdapt = new SMSListAdapter(this, SMS);
            eListAdapt.NotifyDataSetChanged();

            var SMSListView = FindViewById<ListView>(Resource.Id.lvSMSList);
            SMSListView.Adapter = eListAdapt;

        }
        #endregion
    }

    public class MessageObject {
        public string MessageID;
        public string From;
        public string Message;
        public DateTime Date;

        public MessageObject(string MessageID, string From, string Message, DateTime Dateval) {
            this.MessageID = MessageID;
            this.From = From;
            this.Message = Message;
            this.Date = Dateval;
        }
    }

    public class SMSListAdapter : BaseAdapter<MessageObject>
    {
        List<MessageObject> items;
        Activity context;
        public SMSListAdapter(Activity context, List<MessageObject> items) : base()
        {
            this.context = context;
            this.items = items;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override MessageObject this[int position]
        {
            get { return items[position]; }
        }

        public override int Count
        {
            get { return items.Count() - 1; }
        }
        
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];
            View view = convertView;
            if (view == null) // no view to re-use, create new
                view = context.LayoutInflater.Inflate(Resource.Layout.MessageList, null);
            var etxtMessageFromList = view.FindViewById<TextView>(Resource.Id.etxtMessageFromList);
            etxtMessageFromList.Text = item.From;

            var etxtMessageIDList = view.FindViewById<TextView>(Resource.Id.etxtMessageIDList);
            etxtMessageIDList.Text = item.MessageID;

            var etxtMessageValueList = view.FindViewById<TextView>(Resource.Id.etxtMessageValueList);
            etxtMessageValueList.Text = item.Message;
            
            return view;
        }

        
    }
}