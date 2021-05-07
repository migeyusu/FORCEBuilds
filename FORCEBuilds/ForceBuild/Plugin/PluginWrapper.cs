namespace FORCEBuild.Plugin
{
    /// <summary>
    /// 基础插件接口，实现类型必须是无参的
    /// </summary>
    public interface IPlugin
    {
        
    }

    public class PluginWrapper<T> where T : IPlugin, new()
    {

    }
}