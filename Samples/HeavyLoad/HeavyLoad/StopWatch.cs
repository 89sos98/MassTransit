namespace HeavyLoad
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	public class StopWatch
	{
		private readonly List<CheckPoint> _marks = new List<CheckPoint>();
		private DateTime _start;
		private DateTime _stop;

		public void Start()
		{
			_start = DateTime.Now;
		}

		public void Stop()
		{
			_stop = DateTime.Now;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendFormat("Started at {0}\n", _start);

			foreach (CheckPoint mark in _marks)
			{
				mark.ToString(sb);
			}

			sb.AppendFormat("Finished at {0}\n", _stop);
			sb.AppendFormat("Total elapsed time: {0}\n", (_stop - _start));

			return sb.ToString();
		}

		public CheckPoint Mark(string description)
		{
			CheckPoint point = new CheckPoint(description);

			_marks.Add(point);

			return point;
		}
	}

	public class CheckPoint
	{
		private readonly string _description;
		private int _operationCount = 1;
		private readonly DateTime _start;
		private DateTime _stop;

		public CheckPoint(string description)
		{
			_description = description;
			_start = DateTime.Now;
		}

		public void ToString(StringBuilder sb)
		{
			TimeSpan duration = _stop - _start;

			sb.AppendFormat("{0}: {1}", _description, duration);

			if (_operationCount > 1)
			{
				sb.AppendFormat(", /{0} = {1}ms", _operationCount, duration.TotalMilliseconds / _operationCount);
				sb.AppendFormat("{0}  {1}/seconds", Environment.NewLine,  _operationCount / duration.TotalSeconds );
			}

			sb.AppendLine();
		}

		public void Complete(int operationCount)
		{
			_stop = DateTime.Now;

			_operationCount = operationCount;
		}
	}
}