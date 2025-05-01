using Xunit;

namespace BPlusTree.WhiteBoxTests;

/// <summary>
/// a collection of white box tests that simply assert on the output of the program
/// </summary>
public class WhiteBoxTests
{

	[Theory]
	//split a root
	[InlineData(new[] { 1, 2, 3 }, "0: 2,\n1: 1,\n1: 2,3,\n")]
	// split leaf where we need to insert children where index > 0 in the children array
	[InlineData(new[] { 1, 2, 3, 4 }, "0: 2,3,\n1: 1,\n1: 2,\n1: 3,4,\n")]
	// split leaf where we need to insert children at the start of the children array (index == 0)
	[InlineData(new[] { 3, 4, 5, 1, 2 }, "0: 2,4,\n1: 1,\n1: 2,3,\n1: 4,5,\n")]
	// split internal node on left 
	[InlineData(new[] { 5, 6, 7, 4, 3, 2, 1 }, "0: 4,\n1: 2,\n2: 1,\n2: 2,3,\n1: 6,\n2: 4,5,\n2: 6,7,\n")]
	//split internal node on the right
	[InlineData(new[] { 1, 2, 3, 4, 5, 6, 7 }, "0: 3,5,\n1: 2,\n2: 1,\n2: 2,\n1: 4,\n2: 3,\n2: 4,\n1: 6,\n2: 5,\n2: 6,7,\n")]
	public void RootSplit(int[] keys, string expected)
	{
		var tree = new BPTree();
		// ARRANGE AND ACT
		InsertKeys(tree, keys);

		var strTree = tree.PrintTree();

		Console.WriteLine(tree.PrintTree());

		// ASSERT
		Assert.Equal(expected, tree.PrintTree());
	}

	private void InsertKeys(BPTree tree, int[] keys)
	{
		foreach (var key in keys)
		{
			// no need to test values, hence simply resuing key as the value
			tree.InsertVal(key, key);
		}
	}
} 