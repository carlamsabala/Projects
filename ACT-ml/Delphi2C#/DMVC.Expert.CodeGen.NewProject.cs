using System;

namespace DMVCExpertCodeGenNewProject
{
    public interface IOTACreator { }
    
    public interface IOTAProjectCreator { }
    
    public interface IOTAProjectCreator80 { }
    
    public interface IOTAProjectCreator160 { }
    
    public interface IOTAModule { }
    
    public interface IOTAFile { }
    
    public interface IOTAProject
    {
        object ProjectOptions { get; }
    }
    
    public interface IOTAProjectOptionsConfigurations
    {
        IBaseConfiguration BaseConfiguration { get; }
    }
    
    public interface IBaseConfiguration
    {
        bool this[string key] { get; set; }
    }
        public static class Constants
    {
        public const string sConsole = "Console";
        public const string sDelphiPersonality = "Delphi";
        public const string cWin32Platform = "Win32";
        public const string cWin64Platform = "Win64";
    }
    
    public abstract class TNewProject : IOTACreator, IOTAProjectCreator, IOTAProjectCreator80
    {
        protected string FFileName;

        
        public virtual string GetCreatorType() => Constants.sConsole;  
        public bool GetExisting() => false;
        public string GetFileSystem() => "";
        public IOTAModule GetOwner() 
        {
            
            return null;
        }
        public bool GetUnnamed() => true;

        
        public string GetFileName() => FFileName;
        public string GetOptionFileName() => "";
        public bool GetShowSource() => false;
        public virtual void NewDefaultModule() { } 
        public virtual IOTAFile NewOptionSource(string ProjectName) => null;
        public virtual void NewProjectResource(IOTAProject Project) { }
        public abstract IOTAFile NewProjectSource(string ProjectName);

        public virtual string GetProjectPersonality() => Constants.sDelphiPersonality;
        public virtual void NewDefaultProjectModule(IOTAProject Project) { }

        public string FileName
        {
            get => GetFileName();
