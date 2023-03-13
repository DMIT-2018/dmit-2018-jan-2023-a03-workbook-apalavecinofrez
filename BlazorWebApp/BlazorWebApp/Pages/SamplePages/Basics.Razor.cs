namespace BlazorWebApp.Pages.SamplePages
{
    public partial class Basics
    {
        #region Fields
        private string myName;
        private int oddEven;
        #endregion
        //  Method invokes when the component is ready to start
        //      having recieved it's initial paramaters from it's parent in the render tree
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            RandomValue();
            
        }

        private void RandomValue()
        {
            Random rnd = new Random();
            oddEven = rnd.Next(0, 25);
            if (oddEven % 2 == 0)
            {
                myName = $"Alan is even {oddEven}";
            }
            else
            {
                myName = null;
            }
        }
    }
}
