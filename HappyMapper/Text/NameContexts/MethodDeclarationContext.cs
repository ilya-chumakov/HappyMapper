using System;

namespace HappyMapper.Text
{
    internal class VariableContext
    {
        public VariableContext(string type, string name)
        {
            Name = name;
            Type = type.NormalizeTypeName();
        }

        public string Name { get; }
        public string Type { get; }
    }

    internal class MethodDeclarationContext
    {
        public string MethodName { get; }
        public VariableContext Return { get; set; }
        public VariableContext[] Arguments { get; set; }

        public MethodDeclarationContext(string methodName, 
            VariableContext @return, 
            params VariableContext[] arguments)
        {
            MethodName = methodName;
            Arguments = arguments;
            Return = @return;
        }
    }
}