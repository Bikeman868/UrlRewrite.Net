using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Conditions
{
    internal class ConditionList : ICondition
    {
        private CombinationLogic _logic;

        private List<ICondition> _conditions;
        private Func<IRequestInfo, IRuleResult, bool> _testFunc;

        public ConditionList Initialize(CombinationLogic logic)
        {
            _logic = logic;

            switch (logic)
            {
                case CombinationLogic.MatchNone:
                    _testFunc = (ri, rr) => All(ri, rr, false);
                    break;
                case CombinationLogic.MatchAll:
                    _testFunc = (ri, rr) => All(ri, rr, true);
                    break;
                case CombinationLogic.MatchNotAny:
                    _testFunc = (ri, rr) => Any(ri, rr, false);
                    break;
                case CombinationLogic.MatchAny:
                    _testFunc = (ri, rr) => Any(ri, rr, true);
                    break;
                default:
                    _testFunc = (rq, rr) => false;
                    throw new NotImplementedException("Condition list does not know how to combine conditions using " + logic + " logic");
            }
            return this;
        }

        public ConditionList Add(ICondition condition)
        {
            if (_conditions == null) _conditions = new List<ICondition>();

            var conditionList = condition as ConditionList;
            if (conditionList == null || conditionList._logic != _logic)
                _conditions.Add(condition);
            else
                _conditions.AddRange(conditionList._conditions);

            return this;
        }

        public void Initialize(XElement configuration)
        {
        }

        public bool Test(IRequestInfo request, IRuleResult ruleResult)
        {
            return _testFunc(request, ruleResult);
        }

        public override string ToString()
        {
            var count = _conditions == null ? 0 : _conditions.Count;
            return "list of " + count + " conditions";
        }

        public string ToString(IRequestInfo requestInfo)
        {
            return ToString();
        }

        private bool All(IRequestInfo request, IRuleResult ruleResult, bool expected)
        {
            if (_conditions == null || _conditions.Count == 0) return true;

            if (request.ExecutionMode != ExecutionMode.ExecuteOnly)
                request.Log.TraceConditionListBegin(request, _logic);
            
            foreach (var condition in _conditions)
            {
                var isTrue = condition.Test(request, ruleResult);

                if (request.ExecutionMode != ExecutionMode.ExecuteOnly)
                    request.Log.TraceCondition(request, condition, isTrue);

                if (isTrue != expected)
                {
                    if (request.ExecutionMode != ExecutionMode.ExecuteOnly)
                        request.Log.TraceConditionListEnd(request, false);
                    return false;
                }
            }

            if (request.ExecutionMode != ExecutionMode.ExecuteOnly)
                request.Log.TraceConditionListEnd(request, true);

            return true;
        }

        private bool Any(IRequestInfo request, IRuleResult ruleResult, bool expected)
        {
            if (_conditions == null || _conditions.Count == 0) return false;

            if (request.ExecutionMode != ExecutionMode.ExecuteOnly)
                request.Log.TraceConditionListBegin(request, _logic);

            foreach (var condition in _conditions)
            {
                var isTrue = condition.Test(request, ruleResult);

                if (request.ExecutionMode != ExecutionMode.ExecuteOnly)
                    request.Log.TraceCondition(request, condition, isTrue);

                if (isTrue == expected)
                {
                    if (request.ExecutionMode != ExecutionMode.ExecuteOnly)
                        request.Log.TraceConditionListEnd(request, true);
                    return true;
                }
            }

            if (request.ExecutionMode != ExecutionMode.ExecuteOnly)
                request.Log.TraceConditionListEnd(request, false);

            return false;
        }
    }
}
