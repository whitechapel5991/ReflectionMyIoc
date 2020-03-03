namespace MyIoC
{
	//[Export]
	//[ImportConstructor]
	public class Logger
	{
		private readonly CustomerBLL2 custBll;
		public Logger(CustomerBLL2 custBll)
		{
			this.custBll = custBll;
		}
	}
}