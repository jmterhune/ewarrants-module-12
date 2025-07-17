
using System;
using tjc.Modules.eWarrants.Components;
using DotNetNuke.Services.Exceptions;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace tjc.Modules.eWarrants
{
    public partial class EditAnnotation : ewarrantsModuleBase
    {
        private readonly INavigationManager _navigationManager;
        public EditAnnotation()
        {
            _navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
        }
        private void BindList()
        {
            var ctl = new CustomAnnotationController();
            IEnumerable<CustomAnnotation> annotations = ctl.GetCustomAnnotationsByUser(UserId);
            rptAnnotations.DataSource = annotations;
            rptAnnotations.DataBind();
        }
        /// -----------------------------------------------------------------------------
        ///         ''' <summary>
        ///         ''' Page_Load runs when the control is loaded
        ///         ''' </summary>
        ///         ''' <remarks>
        ///         ''' </remarks>
        ///         ''' <history>
        ///         ''' </history>
        ///         ''' -----------------------------------------------------------------------------
        protected void Page_Load(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (!HasJudgeRole)
                    Response.Redirect(_navigationManager.NavigateURL(), true);
                if (Page.IsPostBack == false)
                    BindList();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(_navigationManager.NavigateURL(), true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
        protected void cmdUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                var ctl = new CustomAnnotationController();
                int matches = ctl.GetCustomAnnotationsByUser(UserId).Where(at => at.AnnotationText.ToLower().Trim() == txtAnnotation.Text.ToLower().Trim()).Count();
                if (matches == 0)
                {
                    CustomAnnotation customAnnotation = new CustomAnnotation { AnnotationText = txtAnnotation.Text, UserID = UserId };
                    ctl.CreateCustomAnnotation(customAnnotation);
                    txtAnnotation.Text = "";
                    BindList();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
        protected void cmdDelete_Click(object sender, EventArgs e)
        {
            LinkButton lnk = (LinkButton)sender;
            switch (lnk.CommandName)
            {
                case "Delete":
                    {
                        var ctl = new CustomAnnotationController();
                        ctl.DeleteCustomAnnotation(System.Convert.ToInt32(lnk.CommandArgument));
                        BindList();
                        break;
                    }
            }
        }
    }
}