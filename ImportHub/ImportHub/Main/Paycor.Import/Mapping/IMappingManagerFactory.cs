namespace Paycor.Import.Mapping
{
    public interface IMappingManagerFactory
    {
        void LoadHandlers();
        IMapOperator GetMappingManager(MapType? mapType);
    }
}