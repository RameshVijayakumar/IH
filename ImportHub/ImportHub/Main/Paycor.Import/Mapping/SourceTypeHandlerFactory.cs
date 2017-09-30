namespace Paycor.Import.Mapping
{
    public static class SourceTypeHandlerFactory
    {
        public static ISourceTypeHandler HandleSourceType(MappingFieldDefinition mappingFieldDefinition)
        {
            switch (mappingFieldDefinition.SourceType)
            {
                case SourceTypeEnum.File:
                    return new FileSourceTypeHandler();
                case SourceTypeEnum.Const:
                    return new ConstantSourceTypeHandler();
                case SourceTypeEnum.HeaderAsValue:
                    return new HeaderAsValueTypeHandler();
                default:
                    return null;
            }
        }
    }
}
