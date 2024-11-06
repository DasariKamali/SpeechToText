using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;

namespace PronunciationAssessmentDemo
{
    class Program
    {
        public static AudioConfig CreateAudioConfigFromBytes(byte[] audioBytes)
        {
            var audioStream = new MemoryStream(audioBytes);
            var pushStream = AudioInputStream.CreatePushStream();
            pushStream.Write(audioBytes); 
            pushStream.Close();

            var audioConfig = AudioConfig.FromStreamInput(pushStream);
            return audioConfig;
        }

        public static async Task<float> AssessPronunciation(byte[] audioBytes, string referenceText)
        {
            string subscriptionKey = "<speech_key>";
            string region = "eastus";

            var pronunciationAssessmentConfig = new PronunciationAssessmentConfig(
                referenceText: referenceText,
                gradingSystem: GradingSystem.HundredMark,
                granularity: Granularity.Phoneme,
                enableMiscue: false);

            var audioConfig = CreateAudioConfigFromBytes(audioBytes);
            var speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
            using (var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig))
            {
                pronunciationAssessmentConfig.ApplyTo(speechRecognizer);
                var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
                if (speechRecognitionResult.Reason == ResultReason.RecognizedSpeech)
                {
                    Console.WriteLine("Recognized: " + speechRecognitionResult.Text);
                    var pronunciationAssessmentResult = PronunciationAssessmentResult.FromResult(speechRecognitionResult);
                    Console.WriteLine($"Accuracy Score: {pronunciationAssessmentResult.AccuracyScore}");
                    return (float)pronunciationAssessmentResult.AccuracyScore;
                }
                else
                {
                    Console.WriteLine($"Recognition failed: {speechRecognitionResult.Reason}");
                    return 0;
                }
            }
        }

        static async Task Main(string[] args)
        {
            string audioFilePath = "C:/Users/kamali/Downloads/kamwav.wav";  
            byte[] audioBytes = File.ReadAllBytes(audioFilePath);
            string referenceText = "school"; 
            float accuracyScore = await AssessPronunciation(audioBytes, referenceText);
            Console.WriteLine($"Final Accuracy Score: {accuracyScore}");
        }
    }
}
