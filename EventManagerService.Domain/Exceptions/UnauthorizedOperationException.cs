namespace EventManagerService.Domain.Exceptions
{
    /// <summary>
    /// Исключение, выбрасываемое когда пользователь пытается выполнить операцию без необходимых прав
    /// </summary>
    public class UnauthorizedOperationException : DomainValidationException
    {
        public string Code { get; }
        public Guid UserId { get; }
        public string OperationType { get; }

        public UnauthorizedOperationException(string code, Guid userId, string operationType) : base(code)
        {
            Code = code;
            UserId = userId;
            OperationType = operationType;

            Data["userId"] = userId;
            Data["operationType"] = operationType;
        }

        public UnauthorizedOperationException(string code, Guid userId, string operationType, Exception innerException) : base(code, innerException)
        {
            Code = code;
            UserId = userId;
            OperationType = operationType;

            Data["userId"] = userId;
            Data["operationType"] = operationType;
        }
    }
}
