namespace HomaGames.Geryon.Editor.CodeGen
{
    internal interface IDvrScriptGenerator
    {
        public string GetDvrFileContent(DvrCodeGenModel model);
    }
}