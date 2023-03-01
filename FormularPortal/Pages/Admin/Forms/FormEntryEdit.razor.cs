using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using FormularPortal;
using FormularPortal.Components;
using Plk.Blazor.DragDrop;
using FormPortal.Core.Services;
using FormularPortal.Components.Modals;
using DatabaseControllerProvider;
using Blazor.Pagination;
using Blazor.BootstrapTabs;
using FormularPortal.Core;
using BlazorTooltips;
using vNext.BlazorComponents.FluentValidation;
using FormPortal.Core.Constants;
using FormPortal.Core.Interfaces;
using FormPortal.Core.Models;
using FormPortal.Core.Filters;
using FormPortal.Core.Models.FormElements;
using BlazorContextMenu;
using CKEditor;

namespace FormularPortal.Pages.Admin.Forms
{
    public partial class FormEntryEdit
    {
        [Parameter]
        public int EntryId { get; set; }

        protected override async Task OnParametersSetAsync()
        {

            using IDbController dbController = dbProviderService.GetDbController(AppdatenService.DbProvider, AppdatenService.ConnectionString);
            FormEntry? entry = await formEntryService.GetAsync(EntryId, dbController);

        }
    }
}