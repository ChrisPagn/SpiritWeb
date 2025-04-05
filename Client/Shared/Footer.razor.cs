////@* footer.razor.cs *@
//using Microsoft.AspNetCore.Components;
//using System;
//using System.Globalization;
//using SpiritWeb.Client.Services;

//namespace SpiritWeb.Client.Shared
//{
//    public partial class Footer : ExtendedComponent<Footer>
//    {
     
//        protected override void OnInitialized()
//        {
//            LanguageService.OnLanguageChanged += OnLanguageChanged;
//        }

//        private void OnLanguageChanged()
//        {
//            InvokeAsync(StateHasChanged);
//        }

//        public void Dispose()
//        {
//            LanguageService.OnLanguageChanged -= OnLanguageChanged;
//        }
//    }
//}
