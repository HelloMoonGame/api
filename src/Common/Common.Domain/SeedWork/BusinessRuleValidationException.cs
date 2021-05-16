using System;
using System.Runtime.Serialization;

namespace Common.Domain.SeedWork
{
    [Serializable]
    public class BusinessRuleValidationException : Exception
    {
        public IBusinessRule BrokenRule { get; }

        public string Details { get; }

        public BusinessRuleValidationException(IBusinessRule brokenRule) : base(brokenRule.Message)
        {
            BrokenRule = brokenRule;
            Details = brokenRule.Message;
        }

        protected BusinessRuleValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Details = info.GetString(nameof(Details));
        }
        
        public override void GetObjectData(
            SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Details), Details);
        }

        public override string ToString()
        {
            return $"{BrokenRule.GetType().FullName}: {BrokenRule.Message}";
        }
    }
}