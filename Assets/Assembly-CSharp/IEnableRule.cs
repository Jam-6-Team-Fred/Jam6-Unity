public interface IEnableRule
{
	event EnableRuleStateChangeEvent OnEnableRuleStateChange;

	bool AllowEnable();
}
