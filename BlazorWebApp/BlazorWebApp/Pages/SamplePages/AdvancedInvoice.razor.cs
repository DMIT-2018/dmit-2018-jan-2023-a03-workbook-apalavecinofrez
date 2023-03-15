#nullable disable
using ChinookSystem;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorWebApp.Pages.SamplePages
{
    public partial class AdvancedInvoice
    {
        enum PaymentTypes
        {
            Unknown,
            Cash,
            Chq,
            CreditCard
        }

        #region Fields
        private InvoiceView invoice;
        private string feedBack;
        private int counter = 1;

        //  Holds metadata related to a data editing process,
        //      such as flags to indicate which fields have been modified.
        //      and the current set of validation messages
        private EditContext? editContext;

        //  used to store the validation messages
        private ValidationMessageStore? messageStore;

        #endregion

        #region Properties
        [Inject]
        protected NavigationManager? NavigationManager { get; set; }
        [Parameter]
        public EventCallback<string> OnSelectionChanged { get; set; }
        #endregion
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            invoice = new InvoiceView();
            Random rnd = new Random();
            invoice.InvoiceNo = rnd.Next(1000, 2000);

            //  Edit context needs to be setup after data has been initialized
            //  setup of the edit context to make use of the pament type property
            editContext = new EditContext(invoice);
            //  set the validation to use the HandleValidationRequest event
            editContext.OnValidationRequested += HandleValidationRequested;
            //  setup the message store to track any validation messages
            messageStore = new ValidationMessageStore(editContext);
        }


        //  Handles the validation requested. This allows for custom validation
        //      outside of using the DataAnnotationsValidator
        private void HandleValidationRequested(object? sender, ValidationRequestedEventArgs args)
        {
            //  clear the message stored if there is any existing validation errors
            messageStore?.Clear();

            //  custom validation logic
            //  payment type cannot be set to "Unknown"
            if (invoice.PaymentType != null && invoice.PaymentType == PaymentTypes.Unknown.ToString())
            {
                messageStore?.Add(() => invoice.PaymentType, "Payment Type cannot be set to Unknown");
            }
        }

        //  called when [payment type is clicked]
        private async void OnPaymentTypeClick()
        {
            //  Waiting for payment type to finish changing
            await OnSelectionChanged.InvokeAsync(invoice.PaymentType);
            //  manually call the validation for the edit context
            editContext?.Validate();
        }

        private void HandleSubmit()
        {
            feedBack = $"Submitted Press = {counter++}";
        }
        private void HandleValidSubmit()
        {
            feedBack = $"Valid submission = {counter++}";
        }
        private void HandleInvalidSubmit()
        {
            feedBack = $"InValid submission = {counter++}";
        }
    }
}
