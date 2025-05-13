using System;
using System.Drawing;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace DMVCExpertNewUnitWizardEx
{

    public interface IOTAModuleServices
    {
        IOTAModule CreateModule(TNewControllerUnitEx unit);
    }
    
    public interface IOTAModule
    {
        string FileName { get; }
    }
    
    public interface IOTAProject
    {
        void AddFile(string fileName, bool addToProject);
    }
    
    public static class IDEHelper
    {
        public static IOTAProject GetActiveProject() => null; 
    }
    
    public static class BorlandIDEServices
    {
        public static object Instance { get; } = null;
    }
    
    public class TNewControllerUnitEx
    {
        public TNewControllerUnitEx(JObject configModel, string personality)
        {
            // Initialization logic.
        }
        
        public string FileName { get; } = "NewControllerUnit.cs";
    }
    
    public class TfrmDMVCNewUnit : Form
    {
        public new DialogResult ShowDialog()
        {
            return DialogResult.OK;
        }
        
        public JObject GetConfigModel()
        {
            return new JObject();
        }
    }
    
    public class ExpertsRepositoryProjectWizardWithProc
    {
        public ExpertsRepositoryProjectWizardWithProc(
            string personality,
            string projectHint,
            string unitCaption,
            string wizardId,
            string vendorName,
            string vendorDescription,
            Action executeProc,
            Func<IntPtr> getIconFunc,
            string[] platforms,
            object additionalParam)
        {
            // Store parameters as needed.
        }
        
        public static void Register(ExpertsRepositoryProjectWizardWithProc wizard)
        {
            
            Console.WriteLine("DMVC New Unit Wizard registered.");
        }
    }
    
    public static class IconLoader
    {
        public static IntPtr LoadIcon(IntPtr instance, string iconName)
        {
            return IntPtr.Zero;
        }
    }
    
    public static class TDMVCNewUnitWizard
    {
        private const string sNewDMVCUnitCaption = "DelphiMVCFramework Controller";
        private const string sNewDMVCProjectHint = "Create New DelphiMVCFramework Controller Unit";
        
        private static readonly string[] Platforms = { cWin32Platform, cWin64Platform  };
        private const string cWin32Platform = "Win32";
        private const string cWin64Platform = "Win64";
        
        private static IntPtr HInstance => IntPtr.Zero; 
        
        
        public static void RegisterDMVCNewUnitWizard(string personality)
        {
            ExpertsRepositoryProjectWizardWithProc.Register(
                new ExpertsRepositoryProjectWizardWithProc(
                    personality,
                    sNewDMVCProjectHint,
                    sNewDMVCUnitCaption,
                    "DMVC.Wizard.NewUnitWizard", 
                    "DelphiMVCFramework",
                    "DelphiMVCFramework Team - https://github.com/danieleteti/delphimvcframework",
                    () =>
                    {
                        using (var wizardForm = new TfrmDMVCNewUnit())
                        {
                            if (wizardForm.ShowDialog() == DialogResult.OK)
                            {
                                JObject json = wizardForm.GetConfigModel();
                                IOTAModuleServices moduleServices = BorlandIDEServices.Instance as IOTAModuleServices;
                                IOTAProject project = IDEHelper.GetActiveProject();
                                IOTAModule controllerUnit = moduleServices.CreateModule(new TNewControllerUnitEx(json, personality));
                                if (project != null)
                                {
                                    project.AddFile(controllerUnit.FileName, true);
                                }
                            }
                        }
                    },
                    () =>
                    {
                        return IconLoader.LoadIcon(HInstance, "DMVCNewUnitIcon");
                    },
                    Platforms,
                    null
                )
            );
        }
    }
}
