namespace HappyMapper.AutoMapper.ConfigurationAPI
{
    /// <summary>
    /// Source member configuration options
    /// </summary>
    public interface ISourceMemberConfigurationExpression
    {
        /// <summary>
        /// Ignore this member for configuration validation and skip during mapping
        /// </summary>
        void Ignore();
    }
}