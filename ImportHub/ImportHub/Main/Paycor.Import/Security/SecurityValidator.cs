using System;
using System.Collections.Generic;
using System.Configuration;
#if !DEBUG
using System.Linq; 
#endif
using log4net;
using Paycor.Security.Principal;
//TODO: Missing unit tests

namespace Paycor.Import.Security
{
    public class SecurityValidator
    {
        private const int SystemCheckPrivId = 824;
        private readonly PaycorUserPrincipal _principal;
        private readonly ILog _log;

        public SecurityValidator(ILog log, PaycorUserPrincipal principal)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            Ensure.ThatArgumentIsNotNull(principal, nameof(principal));
            _log = log;
            _principal = principal;
        }

        public bool IsUserAuthorizedForEeImport(int? clientId = null)
        {
            int[] privileges = { PrivilegeConstants.EmployeeImportPrivilegeId };
            var result = HasPrivilege(privileges, clientId);
            return result;
        }

        public bool IsAuthorizedForGlobalMapPrivilege(int? clientId = null)
        {
            int[] privileges = { PrivilegeConstants.GlobalMapPrivilegeId };
            var result = HasPrivilege(privileges, clientId);
            return result;
        }

        public bool IsAuthorizedForClientMapPrivilege(int? clientId = null)
        {
            int[] privileges = { PrivilegeConstants.ClientMapPrivilegeId };
            var result = HasPrivilege(privileges, clientId);
            return result;
        }

        public bool IsAuthorizedForMapManagementPrivilege(int? clientId = null)
        {
            int[] privileges = { PrivilegeConstants.MapManagmentPrivilegeId };
            var result = HasPrivilege(privileges, clientId);
            return result;
        }

        public bool IsAuthorizedForSystemCheck()
        {
            return HasPrivilege(new int[] { SystemCheckPrivId });
        }

        private bool IsAuthenticated()
        {
            if (_principal.Identity.IsAuthenticated)
                return true;

            var errMessage = string.Format(SecurityResource.UserUnAuthenticated);
            _log.Error(errMessage);
            return false;
        }

        private bool HasPrivilege(int[] privileges, int? clientId = null)
        {
            if (!IsAuthenticated())
            {
                _log.Debug("Has Privilige is returning false because the current user is not authenticated.");
                return false;
            }

            var result = clientId.HasValue
                ? _principal.HasAnyPrivilege(privileges, clientId.Value)
                : _principal.HasAnyPrivilege(privileges);

            if (!result)
            {
                _log.DebugFormat("HasAnyPrivilege call has failed{0}.",
                    clientId == null ? string.Empty : " for user attempting to access resources for client ID " + clientId.Value.ToString());
            }
            return result;
        }

        public int[] GetClientIds()
        {          
            try
            {
#if DEBUG
                var devClientAccess = ConfigurationManager.AppSettings["DeveloperClientAccess"];
                var items = new List<int>();
                if (!string.IsNullOrEmpty(devClientAccess))
                {
                    var clientArray = devClientAccess.Split(ImportConstants.Comma);
                    foreach (var client in clientArray)
                    {
                        int result;
                        if (int.TryParse(client, out result))
                        {
                            items.Add(result);
                        }
                    }
                }
                var clients = items.ToArray();
#else
                var clients = _principal.Clients.ToArray();
#endif
                return clients;
            }
            catch (Exception ex)
            {
                _log.Error("An error occured while retrieving the clients for current user", ex);
                throw new Exception("An error occured while retrieving the clients for current user");
            }
        }

        public string GetUser()
        {
            try
            {
                var user = _principal.UserKey;
                return user.ToString();
            }
            catch (Exception ex)
            {
                _log.Error("An error occured while retrieving the current user", ex);
                throw new Exception("An error occured while retrieving the current user");
            }
                 
        }

        public string GetUserName()
        {
            try
            {
                return _principal.FirstName + " " + _principal.LastName;
            }
            catch (Exception ex)
            {
                _log.Error("An error occured while retrieving the current user name", ex);
                throw new Exception("An error occured while retrieving the current user name");
            }

        }
    }
}