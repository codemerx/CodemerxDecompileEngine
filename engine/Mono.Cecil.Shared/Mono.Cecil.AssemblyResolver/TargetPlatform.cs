namespace Mono.Cecil.AssemblyResolver
{
	public enum TargetPlatform
	{
        None = 0,
        CLR_1,
        CLR_2,
		CLR_2_3,
        CLR_3_5,
		CLR_4,
        WPF,
		Silverlight,
		WindowsPhone,
		WindowsCE,
        WinRT,
		NetCore,
		/* AGPL */
		NetStandard,
		/* End AGPL */
		Xamarin
	}
}
