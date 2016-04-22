using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources.Core;
using Windows.ApplicationModel.VoiceCommands;

namespace RuntimeComponentCortana
{
    public sealed class CortanaDialogFlow : IBackgroundTask
    {
        /// <summary>
        /// the service connection is maintained for the lifetime of a cortana session, once a voice command
        /// has been triggered via Cortana.
        /// </summary>
        VoiceCommandServiceConnection _voiceServiceConnection;

        /// <summary>
        /// Lifetime of the background service is controlled via the BackgroundTaskDeferral object, including
        /// registering for cancellation events, signalling end of execution, etc. Cortana may terminate the 
        /// background service task if it loses focus, or the background task takes too long to provide.
        /// 
        /// Background tasks can run for a maximum of 30 seconds.
        /// </summary>
        BackgroundTaskDeferral _serviceDeferral;


        /// <summary>
        /// The context for localized strings.
        /// </summary>
        ResourceContext _cortanaContext;


        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _serviceDeferral = taskInstance.GetDeferral();

            // Register to receive an event if Cortana dismisses the background task. This will
            // occur if the task takes too long to respond, or if Cortana's UI is dismissed.
            // Any pending operations should be cancelled or waited on to clean up where possible.
            taskInstance.Canceled += OnTaskCanceled;

            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;


            // Select the system language, which is what Cortana should be running as.
            _cortanaContext = ResourceContext.GetForViewIndependentUse();

            // This should match the uap:AppService and VoiceCommandService references from the 
            // package manifest and VCD files, respectively. Make sure we've been launched by
            // a Cortana Voice Command.
            if (triggerDetails != null && triggerDetails.Name == "CortanaDialogFlow")
            {
                try
                {
                    _voiceServiceConnection =
                        VoiceCommandServiceConnection.FromAppServiceTriggerDetails(
                            triggerDetails);

                    _voiceServiceConnection.VoiceCommandCompleted += OnVoiceCommandCompleted;

                    // GetVoiceCommandAsync establishes initial connection to Cortana, and must be called prior to any 
                    // messages sent to Cortana. Attempting to use ReportSuccessAsync, ReportProgressAsync, etc
                    // prior to calling this will produce undefined behavior.
                    VoiceCommand voiceCommand = await _voiceServiceConnection.GetVoiceCommandAsync();

                    // Depending on the operation (defined in TestCortana:TestCortanaCommands.xml)
                    // perform the appropriate command.
                    switch (voiceCommand.CommandName)
                    {
                        case "changeAmbiance":
                            await SendCompletionMessageForAmbiance();
                            break;
                        default:
                            // As with app activation VCDs, we need to handle the possibility that
                            // an app update may remove a voice command that is still registered.
                            // This can happen if the user hasn't run an app since an update.
                            LaunchAppInForeground();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Handling Voice Command failed " + ex.ToString());
                }
            }
        }

        private async Task SendCompletionMessageForAmbiance()
        {
            await ShowProgressScreen("Changement d'ambiance ...");


            var userPrompt = new VoiceCommandUserMessage();
            userPrompt.DisplayMessage =
                userPrompt.SpokenMessage = "Quelle ambiance voulez vous choisir ?";

            var userReprompt = new VoiceCommandUserMessage();
            userReprompt.DisplayMessage =
                userReprompt.SpokenMessage = "Quelle ambiance ?";

            var ambianceContentTiles = new List<VoiceCommandContentTile>();

            var ambianceWork = new VoiceCommandContentTile();
            ambianceWork.ContentTileType = VoiceCommandContentTileType.TitleWithText;
            ambianceWork.Title = "Ambiance de travail";
            ambianceWork.TextLine1 = "Permet de mettre les lampes en vert";
            ambianceWork.AppContext = "work";

            var ambianceCool = new VoiceCommandContentTile();
            ambianceCool.ContentTileType = VoiceCommandContentTileType.TitleWithText;
            ambianceCool.Title = "Ambiance de détente";
            ambianceCool.TextLine1 = "Permet de mettre les lampes en bleu";
            ambianceCool.AppContext = "cool";

            ambianceContentTiles.Add(ambianceWork);
            ambianceContentTiles.Add(ambianceCool);

            var response = VoiceCommandResponse.CreateResponseForPrompt(userPrompt, userReprompt, ambianceContentTiles);

            var voiceCommandDisambiguationResult = await
              _voiceServiceConnection.RequestDisambiguationAsync(response);
            if (voiceCommandDisambiguationResult != null)
            {
                string ambiance = voiceCommandDisambiguationResult.SelectedItem.AppContext as string;
                userPrompt.DisplayMessage = userPrompt.SpokenMessage = "Activer l'ambiance  " + ambiance;
                userReprompt.DisplayMessage = userReprompt.DisplayMessage = "Voulez vous activer l'ambiance " + ambiance + "?";
                response = VoiceCommandResponse.CreateResponseForPrompt(userPrompt, userReprompt);

                var voiceCommandConfirmation = await _voiceServiceConnection.RequestConfirmationAsync(response);

                // If RequestConfirmationAsync returns null, Cortana's UI has likely been dismissed.
                if (voiceCommandConfirmation != null)
                {
                    if (voiceCommandConfirmation.Confirmed == true)
                    {
                        await ShowProgressScreen("Activation de l'ambiance");
                        var dataAccess = new DataAccess.Light();
                        // Job to Active Ambiance
                        switch (ambiance)
                        {
                            case "work":
                                await dataAccess.On(new Light() { State = true, LightId = 1, Color = new Color() { B = 1 } });
                                break;
                            case "cool":
                                await dataAccess.On(new Light() { State = true, LightId = 1, Color = new Color() { G = 1 } });
                                break;
                            default:
                                break;
                        }

                        var userMessage = new VoiceCommandUserMessage();

                        userMessage.DisplayMessage = userMessage.SpokenMessage = "L'ambiance " + ambiance + " a été activée";
                        response = VoiceCommandResponse.CreateResponse(userMessage);
                        await _voiceServiceConnection.ReportSuccessAsync(response);
                    }
                }
            }
        }

        /// <summary>
        /// Show a progress screen. These should be posted at least every 5 seconds for a 
        /// long-running operation, such as accessing network resources over a mobile 
        /// carrier network.
        /// </summary>
        /// <param name="message">The message to display, relating to the task being performed.</param>
        /// <returns></returns>
        private async Task ShowProgressScreen(string message)
        {
            var userProgressMessage = new VoiceCommandUserMessage();
            userProgressMessage.DisplayMessage = userProgressMessage.SpokenMessage = message;

            VoiceCommandResponse response = VoiceCommandResponse.CreateResponse(userProgressMessage);
            await _voiceServiceConnection.ReportProgressAsync(response);
        }

        /// <summary>
        /// Provide a simple response that launches the app. Expected to be used in the
        /// case where the voice command could not be recognized (eg, a VCD/code mismatch.)
        /// </summary>
        private async void LaunchAppInForeground()
        {
            var userMessage = new VoiceCommandUserMessage();
            userMessage.SpokenMessage = "lancement de l'application ...";

            var response = VoiceCommandResponse.CreateResponse(userMessage);

            response.AppLaunchArgument = "";

            await _voiceServiceConnection.RequestAppLaunchAsync(response);
        }

        /// <summary>
        /// Handle the completion of the voice command. Your app may be cancelled
        /// for a variety of reasons, such as user cancellation or not providing 
        /// progress to Cortana in a timely fashion. Clean up any pending long-running
        /// operations (eg, network requests).
        /// </summary>
        /// <param name="sender">The voice connection associated with the command.</param>
        /// <param name="args">Contains an Enumeration indicating why the command was terminated.</param>
        private void OnVoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            if (_serviceDeferral != null)
            {
                _serviceDeferral.Complete();
            }
        }

        /// <summary>
        /// When the background task is cancelled, clean up/cancel any ongoing long-running operations.
        /// This cancellation notice may not be due to Cortana directly. The voice command connection will
        /// typically already be destroyed by this point and should not be expected to be active.
        /// </summary>
        /// <param name="sender">This background task instance</param>
        /// <param name="reason">Contains an enumeration with the reason for task cancellation</param>
        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            System.Diagnostics.Debug.WriteLine("Task cancelled, clean up");
            if (_serviceDeferral != null)
            {
                //Complete the service deferral
                _serviceDeferral.Complete();
            }
        }
    }
}
