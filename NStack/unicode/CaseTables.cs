using System;

namespace NStack {
	public partial class Unicode {

		/// <summary>
		/// Special casing rules for Turkish.
		/// </summary>
		public static SpecialCase TurkishCase = new SpecialCase (
			new CaseRange [] {
				new CaseRange (0x0049, 0x0049, 0, 0x131 - 0x49, 0),
				new CaseRange (0x0069, 0x0069, 0x130 - 0x69, 0, 0x130 - 0x69),
				new CaseRange (0x0130, 0x0130, 0, 0x69 - 0x130, 0),
				new CaseRange (0x0131, 0x0131, 0x49 - 0x131, 0, 0x49 - 0x131),
			});
	}

}
