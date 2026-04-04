using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWSQOL.Modules
{
    /// <summary>
    /// This class forces speedrun validation to be enabled and does not allow it to be disabled.
    /// </summary>
    public class ForceValidation
    {
        public static void Apply()
        {
            On.Options.ctor += Options_ctor;
            On.Options.ApplyOption += Options_ApplyOption;
            On.Options.FromString += Options_FromString;
            On.Menu.OptionsMenu.InitMiscCheckboxes += OptionsMenu_InitMiscCheckboxes;
        }

        /// <summary>
        /// Grey out the validation option at the only point it is constructed so it may not be disabled (happens after all options hooks have already set it).
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        private static void OptionsMenu_InitMiscCheckboxes(On.Menu.OptionsMenu.orig_InitMiscCheckboxes orig, Menu.OptionsMenu self)
        {
            orig(self);
            self.validationCheckbox.buttonBehav.greyedOut = true;
        }

        // All 3 meaningful locations where the validation could be set. Overwrite with true.
        private static void Options_ctor(On.Options.orig_ctor orig, Options self, RainWorld rainWorld)
        {
            orig(self, rainWorld);
            self.validation = true;
        }

        private static void Options_FromString(On.Options.orig_FromString orig, Options self, string s)
        {
            orig(self, s);
            self.validation = true;
        }

        private static bool Options_ApplyOption(On.Options.orig_ApplyOption orig, Options self, string[] splt2)
        {
            bool origRet = orig(self, splt2);
            self.validation = true;
            return origRet;
        }
    }
}
