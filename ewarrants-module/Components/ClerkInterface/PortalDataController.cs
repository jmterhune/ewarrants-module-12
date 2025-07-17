using DotNetNuke.Services.Exceptions;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;

namespace tjc.Modules.eWarrants.ClerkInterface
{
    public class PortalController : DnnApiController
    {

        [HttpGet]
        [ActionName("SetPortalStatus")]
        [DnnAuthorize(AuthTypes = "JWT")]
        public PortalStatusResponse SetPortalStatus(int warrantId, int status)
        {
            try
            {
                var ctl = new PortalWarrantController();
                ctl.UpdatePortalStatus(warrantId, status);
                return new PortalStatusResponse { ResponseResult= 1 , ResponseError = string.Empty };
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return new PortalStatusResponse { ResponseResult = 0, ResponseError=ex.Message };
            }
        }
        [HttpGet]
        [ActionName("GetPortalWarrants")]
        [DnnAuthorize(AuthTypes = "JWT")]
        public IEnumerable<PortalWarrant> GetPortalWarrants()
        {
            try
            {
                
                var ctl = new PortalWarrantController();
              return  ctl.GetPortalWarrants();
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return null;
            }
        }
        [HttpGet]
        [ActionName("GetFile")]
        [DnnAuthorize(AuthTypes = "JWT")]
        public WarrantFile GetFile(int fileId)
        {
            try
            {

                var ctl = new PortalWarrantController();
                return ctl.GetWarrantFile(fileId);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return null;
            }
        }
        [HttpGet]
        [ActionName("GetSignedWarrants")]
        [DnnAuthorize(AuthTypes = "JWT")]
        public IEnumerable<SignedWarrant> GetSignedWarrants()
        {
            try
            {
                var ctl = new PortalWarrantController();
                return ctl.GetSignedWarrants();
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return null;
            }
        }

      
    }
    public class PortalStatusResponse
    {
        public int ResponseResult { get; set; }
        public string ResponseError { get; set; }
    }
}