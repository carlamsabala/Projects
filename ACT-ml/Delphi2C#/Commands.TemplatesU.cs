using System.Collections.Generic;
using CommonsU;
using ProjectGeneratorU;
using CommandsU;

namespace CommandsTemplatesU
{
    public static class CommandsTemplates
    {
        public static void FillProgramTemplates(TMVCCodeGenerator gen)
        {
            gen.Commands.AddRange(new List<ICommand>
            {
                new TUnitProgramCommand(),
                new TUnitRunServerProcBody(),
                new TUnitMainBeginEndCommand()
            });
        }

        public static void FillControllerTemplates(TMVCCodeGenerator gen)
        {
            gen.Commands.AddRange(new List<ICommand>
            {
                new TUnitControllerCommand(),
                new TUnitControllerEntityDeclarationCommand(),
                new TUnitControllerControllerDeclarationCommand(),
                new TUnitFooterCommand()
            });
        }

        public static void FillWebModuleTemplates(TMVCCodeGenerator gen)
        {
            gen.Commands.AddRange(new List<ICommand>
            {
                new TUnitWebModuleDeclarationCommand()
            });
        }

        public static void FillJSONRPCTemplates(TMVCCodeGenerator gen)
        {
            gen.Commands.AddRange(new List<ICommand>
            {
                new TUnitJSONRPCDeclarationCommand()
            });
        }

        public static void FillWebModuleDFMTemplates(TMVCCodeGenerator gen)
        {
            gen.Commands.AddRange(new List<ICommand>
            {
                new TWebModuleDFMCommand()
            });
        }
    }
}
