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

namespace GDSSAssistant.Activities
{
    [Activity(Label = "GDSS: Call Assistant")]
    public class PhoneCallActivity : Activity, TextToSpeech.IOnInitListener
    {
        private TextToSpeech textToSpeech;
        //Context context;
        private string ActivityStatus;
        private string NumberToCall;
        private bool NeedToSpeak;
        private string MessageToSpeak;

        private readonly int NeedLang = 103;
        private Java.Util.Locale lang;

        //int count = 1;
        private bool isRecording;
        private readonly int VOICE = 10;
        private TextView STTtextBox;
        private ImageButton recButton;
        //private ImageButton callButton;
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

        #region "Initialize Text-To-Speech"
        private void InitTTSMethod()
        {
            btnSayIt = FindViewById<Button>(Resource.Id.btnSpeakPhoneForm);
            var editWhatToSay = FindViewById<EditText>(Resource.Id.etxtTTSPhoneForm);
            
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

        private void InvokeRead()
        {
            if (NeedToSpeak)
            {

                var btnSpeak = FindViewById<Button>(Resource.Id.btnSpeakPhoneForm);
                btnSpeak.PerformClick();
                    
                var btnAction = FindViewById<ImageButton>(Resource.Id.imgBtnListenPhoneForm);
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


            var editWhatToSay = FindViewById<EditText>(Resource.Id.etxtTTSPhoneForm);
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

            var btnAction = FindViewById<ImageButton>(Resource.Id.imgBtnListenPhoneForm);
            btnAction.SetImageResource(Resource.Drawable.micspeak);
            //@android:drawable/presence_audio_online + Android.Resource.Drawable.PresenceAudioOnline + Android.Resource.Drawable.IcDialogInfo

            NeedToSpeak = false;
        }

        #endregion

        #region "Initialize Speech to Text Library"
        private void InitSTTMethod()
        {

            //var tcs = new TaskCompletionSource<string>();

            // get the resources from the layout
            recButton = FindViewById<ImageButton>(Resource.Id.imgBtnListenPhoneForm);

            STTtextBox = FindViewById<TextView>(Resource.Id.etxtToCallPhoneForm);

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

                recButton.Click += delegate {
                    if (NeedToSpeak)
                    {
                        InvokeRead();
                    }
                    else {
                        if (AllInputsValid())
                        {
                            PerformPhoneCall(0);
                        }
                        else {
                            ListenToSpeech();
                        }
                    }
                };
        }
        private bool AllInputsValid() {
            if (!string.IsNullOrEmpty(NumberToCall)) { return true; } else { return false; }
        }


        private void ListenToSpeech()
        {
            // create the intent and start the activity
            var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);

            // put a message on the modal dialog
            voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, "GDSS Phone Call Voice Input");
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
        #endregion
        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.PhoneForm);
            isRecording = false;

            //initialize speech to text (input)
            InitSTTMethod();
            //initialize text to speech (read/speak)
            InitTTSMethod();

            var backButton = FindViewById<Button>(Resource.Id.btnBackPhoneForm);
            backButton.Click += delegate { Finish(); };

            var btnResetPhoneForm = FindViewById<Button>(Resource.Id.btnResetPhoneForm);
            btnResetPhoneForm.Click += delegate
            {
                Finish();
                StartActivity(typeof(PhoneCallActivity));
            };

            ActivityStatus = "Created";

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
                SetReadMessage(GetString(Resource.String.PhoneCallAskWhoToCall));
            }

        }

        protected override void OnStart()
        {
            base.OnStart();
            
        }

        protected override void OnActivityResult(int req, Result res, Intent data)
        {
            STTtextBox = FindViewById<TextView>(Resource.Id.etxtToCallPhoneForm);
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
                        //IF CONTACTTOCALL-VAR IS BLANK OR NO VALUE, target call.
                        if (string.IsNullOrEmpty(NumberToCall))
                        {
                            STTtextBox.Text = textInput;
                            NumberToCall = textInput;

                            string strPhone = SearchContactList_PhoneNumber(textInput);

                            if (strPhone == textInput) {
                                SetReadMessage("Contact " + textInput + " not found. Press the button to call anyway");
                            } else {
                                SetReadMessage("Press the button to call " + textInput);
                            }
                            
                            //NeedToSpeak = false;
                        }
                        else {
                            if (!string.IsNullOrEmpty(NumberToCall))
                            {
                            ActivityStatus = "Calling on the Phone.";
                            //Console.WriteLine("Call on Phone: " + NumberToCall);
                                    
                            PerformPhoneCall(0);
                            NumberToCall = "";
                            STTtextBox.Text = "";
                            }
                        }
                    }
                    else { 
                        SetReadMessage("Unrecognized Command");
                    }
                }

            }

            base.OnActivityResult(req, res, data);

        }

        private void SetReadMessage(string ReadMessage)
        {
            NeedToSpeak = true;
            var etxtSpeak = FindViewById<EditText>(Resource.Id.etxtTTSPhoneForm);
            etxtSpeak.Text = ReadMessage;
            MessageToSpeak = ReadMessage;

            var btnAction = FindViewById<ImageButton>(Resource.Id.imgBtnListenPhoneForm);
            btnAction.SetImageResource(Resource.Drawable.micread);
            //@android:drawable/presence_audio_online + Android.Resource.Drawable.PresenceAudioOnline + Android.Resource.Drawable.IcDialogInfo

        }


        #region "Component Methods"
        private string SearchContactList_PhoneNumber(string Name)
        {

            var uri = ContactsContract.Contacts.ContentUri;
            var contactPhoneInfoUri = ContactsContract.CommonDataKinds.Phone.ContentUri;
            string contactID = "";

            string[] projection = { ContactsContract.Contacts.InterfaceConsts.Id, ContactsContract.Contacts.InterfaceConsts.DisplayName };

            var cursor = Application.Context.ContentResolver.Query(ContactsContract.Contacts.ContentUri, null,
                @ContactsContract.Contacts.InterfaceConsts.DisplayName + " like '%" + Name + "%'", null, null);
                //ManagedQuery(uri, projection, null, null, null);



            if (cursor.MoveToFirst())
            {
                bool contactfound = false;
                do
                {
                    //contactList.Add(cursor.GetString( cursor.GetColumnIndex(projection[1])));
                    //(cursor.GetString(cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName)).ToString().ToLower().Contains(Name.ToLower()))
                    if (cursor.GetString(cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName)).ToLower() == Name.ToLower())
                    { 
                    contactID = cursor.GetString(cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.Id));
                        //cursor.GetString(cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.Id));

                    //sms
                    //var sms = Application.Context.ContentResolver.Query(Telephony.Sms.Inbox.ContentUri,null,null, Telephony.Sms.Inbox.InterfaceConsts.);
                    
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
                };
                    //Console.WriteLine(cursor.GetString(
                    //    cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.Id)).ToString() + ":" + cursor.GetString(cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName)).ToString());
                } while (cursor.MoveToNext() || contactfound);
            }
            cursor.Close();
            return Name;
        }
        private void PerformPhoneCall(long MillisecValue)
        {
            string retListen = NumberToCall ?? "";

            string CallReceiverCallNumber = SearchContactList_PhoneNumber(retListen);
            var uri = Android.Net.Uri.Parse(@"tel:" + CallReceiverCallNumber);
            var VoiceTriggeredCallIntent = new Intent(Intent.ActionCall, uri);
            StartActivity(VoiceTriggeredCallIntent);

            NumberToCall = "";
            SetReadMessage("Press button and say the Name or the Number of the contact you would like to call.");
        }

        #endregion
    }
}