using DatabaseControllerProvider;
using FormPortal.Core.Filters;
using FormPortal.Core.Services;
using Microsoft.AspNetCore.Components;

namespace FormPortal.Pages.Account
{
    public partial class AccountEntries
    {
        public FormEntryFilter Filter { get; set; } = new();

        [Parameter]
        public int Page { get; set; } = 1;
        protected override async Task OnParametersSetAsync()
        {
            if (Page < 1)
            {
                Page = 1;
            }

            Filter.PageNumber = Page;

            using IDbController dbController = dbProviderService.GetDbController(AppdatenService.ConnectionString);
            var user = await authService.GetUserAsync(dbController);
            Filter.UserId = user?.UserId ?? 0;
        }
    }
}