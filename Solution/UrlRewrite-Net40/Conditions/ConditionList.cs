using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Conditions;
using UrlRewrite.Interfaces.Rules;

namespace UrlRewrite.Conditions
{
    public class ConditionList : IConditionList
    {
        private CombinationLogic _logic;
        private bool _trackAllCaptures;

        private List<ICondition> _conditions;
        private Func<IRequestInfo, IRuleResult, bool> _testFunc;

        public IConditionList Initialize(CombinationLogic logic, bool trackAllCaptures = false)
        {
            _logic = logic;
            _trackAllCaptures = trackAllCaptures;

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

        public IConditionList Add(ICondition condition)
        {
            if (_conditions == null) _conditions = new List<ICondition>();

            var conditionList = condition as ConditionList;
            if (conditionList == null 
                || conditionList._logic != _logic 
                || conditionList._trackAllCaptures != _trackAllCaptures)
                _conditions.Add(condition);
            else if (conditionList._conditions != null)
                _conditions.AddRange(conditionList._conditions);

            return this;
        }

        public ICondition Initialize(XElement configuration, IValueGetter valueGetter)
        {
            return this;
        }

        public bool Test(IRequestInfo request, IRuleResult ruleResult)
        {
            if (_trackAllCaptures)
                ruleResult.Properties.Set(true, "trackAllCaptures");

            var result = _testFunc(request, ruleResult);
            
            if (_trackAllCaptures)
                ruleResult.Properties.Set(false, "trackAllCaptures");

            return result;
        }

        public override string ToString()
        {
            var count = _conditions == null ? 0 : _conditions.Count;
            return "list of " + count + " conditions" + (_trackAllCaptures ? " tracking all captures" : "");
        }

        public string ToString(IRequestInfo requestInfo)
        {
            return ToString();
        }

        private bool All(IRequestInfo request, IRuleResult ruleResult, bool expected)
        {
            if (_conditions == null || _conditions.Count == 0) return true;

            if (request.ExecutionMode != ExecutionMode.ExecuteOnly)
                request.Log.TraceConditionListBegin(request, this);
            
            foreach (var condition in _conditions)
            {
                var isTrue = condition.Test(request, ruleResult);

                if (request.ExecutionMode != ExecutionMode.ExecuteOnly)
                    request.Log.TraceCondition(request, condition, isTrue);

                if (isTrue != expected)
                {
                    if (request.ExecutionMode != ExecutionMode.ExecuteOnly)
                        request.Log.TraceConditionListEnd(request, this, false);
                    return false;
                }
            }

            if (request.ExecutionMode != ExecutionMode.ExecuteOnly)
                request.Log.TraceConditionListEnd(request, this, true);

            return true;
        }

        private bool Any(IRequestInfo request, IRuleResult ruleResult, bool expected)
        {
            if (_conditions == null || _conditions.Count == 0) return false;

            if (request.ExecutionMode != ExecutionMode.ExecuteOnly)
                request.Log.TraceConditionListBegin(request, this);

            foreach (var condition in _conditions)
            {
                var isTrue = condition.Test(request, ruleResult);

                if (request.ExecutionMode != ExecutionMode.ExecuteOnly)
                    request.Log.TraceCondition(request, condition, isTrue);

                if (isTrue == expected)
                {
                    if (request.ExecutionMode != ExecutionMode.ExecuteOnly)
                        request.Log.TraceConditionListEnd(request, this, true);
                    return true;
                }
            }

            if (request.ExecutionMode != ExecutionMode.ExecuteOnly)
                request.Log.TraceConditionListEnd(request, this, false);

            return false;
        }

        public void Describe(TextWriter writer, string indent, string indentText)
        {
            if (_conditions != null && _conditions.Count > 0)
            {
                writer.WriteLine(indent + _logic + " these conditions" + (_trackAllCaptures ? " tracking all captures:" : ":"));
                indent += indentText;
                foreach (var condition in _conditions)
                    condition.Describe(writer, indent, indentText);
            }
        }
    }
}
