
using DotNetNuke.Services.Exceptions;
using System;
using System.Linq;
using System.Web.UI.WebControls;
using tjc.Modules.eWarrants.Components;

namespace tjc.Modules.eWarrants
{
    public partial class EditAgency : ewarrantsModuleBase
    {
        #region Methods
        private void BindControls(AgencyController ctl)
        {
            drpParentAgency.DataSource = ctl.GetAgencies(ModuleId).Where(a => a.AgencyID != AgencyId);
            drpParentAgency.DataBind();
            drpParentAgency.Items.Insert(0, new ListItem("< Select Parent >", ""));

            var cCtl = new CountyController();
            var colCounties = cCtl.GetCounties(ModuleId);
            chkCounties.DataSource = colCounties;
            chkCounties.DataBind();

            if (AgencyId > 0)
            {
                var colSelectedCounties = cCtl.GetCountiesByAgency(AgencyId).Select(cty => cty.CountyName);
                foreach (var item in colSelectedCounties)
                {
                    ListItem listItem = this.chkCounties.Items.FindByText(item);
                    if (listItem != null)
                        listItem.Selected = true;
                }
            }
        }
        #endregion
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
                if (Page.IsPostBack == false)
                {
                    lnkCancel.NavigateUrl = EditUrl("adminAgency");
                    var ctl = new AgencyController();
                    BindControls(ctl);
                    if (AgencyId > 0)
                    {
                        Agency objAgency = ctl.GetAgency(AgencyId, ModuleId);
                        if (objAgency != null)
                        {
                            txtAgencyName.Text = objAgency.AgencyName;
                            txtEmail.Text = objAgency.EmailAddress;
                            drpParentAgency.SelectedValue = objAgency.ParentAgencyID.ToString();
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (Page.IsValid)
                {
                    Agency agency = null;
                    int agencyId = AgencyId;
                    var ctl = new AgencyController();
                    if (AgencyId > 0)
                        agency = ctl.GetAgency(agencyId, ModuleId);
                    else
                        agency = new Agency();

                    agency.AgencyName = txtAgencyName.Text;
                    agency.EmailAddress = txtEmail.Text;
                    agency.ModuleID = ModuleId;
                    Int32.TryParse(drpParentAgency.SelectedValue, out int parentAgencyId);
                    agency.ParentAgencyID = parentAgencyId;
                    if (agencyId > 0)
                    {
                        ctl.UpdateAgency(agency);
                        ctl.DeleteAgencyCounties(agencyId);
                    }
                    else
                    {
                        ctl.CreateAgency(agency);
                        agencyId = agency.AgencyID;
                    }
                    foreach (ListItem li in chkCounties.Items)
                    {
                        Int32.TryParse(li.Value, out int countyId);
                        if (li.Selected)
                        {
                            AgencyCounty agencyCounty = new AgencyCounty { AgencyID = agencyId, CountyID = countyId };
                            ctl.CreateAgencyCounty(agencyCounty);
                        }
                    }
                    Response.Redirect(EditUrl("adminAgency"));
                }
                else
                {
                    string message = "Please review your entries making sure to complete all required fields.";
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, message, DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.BlueInfo);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}