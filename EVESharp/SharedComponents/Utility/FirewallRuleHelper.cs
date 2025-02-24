using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedComponents.EVE;

namespace SharedComponents.Utility
{
    public static class FirewallRuleHelper
    {
        public const string FW_RULE_NAME_TQ = "E#-Blocking-Rule-TQ";
        public const string FW_RULE_NAME_SISI = "E#-Blocking-Rule-SISI";

        public static bool CheckIfRuleNameExists(string name)
        {
            var res = Util.ExecCommand($"netsh advfirewall firewall show rule name=all | find \"{name}\"");
            Debug.WriteLine($"Res: [{res}] Length [{res.Length}]");
            return res.Length > 0;
        }

        public static void AddBlockingRule(string name, string pathToExe)
        {
            if (!CheckIfRuleNameExists(name))
            {
                Util.ExecCommand($"netsh advfirewall firewall add rule name=\"{name}\" dir=out action=block program=\"{pathToExe}\"");
                Debug.WriteLine($"FWRule added name: [{name}]");
            }
            else
            {
                Debug.WriteLine($"FWRule could not be added name: [{name}]");
            }
        }


        public static void AddIPBlockingRule(string name, string pathToExe, string ips)
        {
            if (!CheckIfRuleNameExists(name))
            {
                Util.ExecCommand($"netsh advfirewall firewall add rule name=\"{name}\" dir=out action=block program=\"{pathToExe}\" protocol=ANY remoteip=\"{ips}\"");
                Debug.WriteLine($"FWRule added name: [{name}]");
            }
            else
            {
                Debug.WriteLine($"FWRule could not be added name: [{name}]");
            }
        }


        public static bool CheckIfIPBlockingRuleMatch(string name, string filter, string evePath)
        {
            // this verification does not work properly with consecutive ips, the windows firewall puts single ips at the end
            var res = Util.ExecCommand($"netsh advfirewall firewall show rule name=\"{name}\" | find \"{filter}\"");
            var res2 = Util.ExecCommand($"netsh advfirewall firewall show rule name=\"{name}\" verbose | find \"{evePath}\"");
            return res.Length > 0 && res2.Length > 0;
        }

        public static bool AddAnIndividualFwBlockingRule(string fwRuleName, string pathToExe, string description)
        {
            if (!CheckIfRuleNameExists(fwRuleName))
            {
                Cache.Instance.Log("Enable Windows Firewall");
                Util.ExecCommand($"netsh advfirewall set allprofiles state on");
                Cache.Instance.Log("Add Firewall Rule to block [" + description + "] from using the local connection");
                Util.ExecCommand($"netsh advfirewall firewall add rule name=\"{fwRuleName}\" dir=out action=block program=\"{pathToExe}\"");
                Debug.WriteLine("FWRule added name: [" + fwRuleName + "]");
                return true;
            }

            Cache.Instance.Log("Enable Windows Firewall");
            Util.ExecCommand($"netsh advfirewall set allprofiles state on");
            Cache.Instance.Log("Firewall Rule to block [" + description + "] from using the local connection exists and is now enabled");
            Util.ExecCommand($"netsh advfirewall firewall set rule name=\"{fwRuleName}\" new enable=yes");
            return false;
        }

        public static void RemoveRule(string name)
        {
            if (CheckIfRuleNameExists(name))
            {
                Util.ExecCommand($"netsh advfirewall firewall del rule name=\"{name}\"");
                Debug.WriteLine($"FWRule deleted name: [{name}]");
            }
            else
            {
                Debug.WriteLine($"Could not delete FWRule name: [{name}]");
            }
        }
    }
}
