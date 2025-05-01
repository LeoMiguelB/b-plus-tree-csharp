using System.Text;

BPTree t = new BPTree() { Order = 3 };

//split a root
//t.InsertVal(1, 1);
//t.InsertVal(2, 2);
//t.InsertVal(3, 3);

// split leaf where we need to insert children where index > 0 in the children array
//t.InsertVal(1, 1);
//t.InsertVal(2, 2);
//t.InsertVal(3, 3);
//t.InsertVal(4, 4);

// split leaf where we need to insert children at the start of the children array (index == 0)
//t.InsertVal(3, 3);
//t.InsertVal(4, 4);
//t.InsertVal(5, 5);
//t.InsertVal(1, 1);
//t.InsertVal(2, 2);

// split internal node on left 
//t.InsertVal(5, 5);
//t.InsertVal(6, 6);
//t.InsertVal(7, 7);
//t.InsertVal(4, 4);
//t.InsertVal(3, 3);
//t.InsertVal(2, 2);
//t.InsertVal(1, 1);

//split internal node on the right
t.InsertVal(1, 1);
t.InsertVal(2, 2);
t.InsertVal(3, 3);
t.InsertVal(4, 4);
t.InsertVal(5, 5);
t.InsertVal(6, 6);
t.InsertVal(7, 7);

t.PrintTree();

public class BPTree
{
	public int Order { get; set; }

	private BPlusNode _root;

	public BPTree(int order = 3)
	{
		this.Order = order;
		_root = new BPlusNode();
	}

	public bool InsertVal(int key, int val)
	{
		// percolate down the tree from root to find where the kv should be
		BPlusNode node = Find(key);

		// *small optimization
		// before we mutate the node, going to do a simple check if the key already exist
		// if it already exist then we don't have to consider "splitting" or "merging"
		bool containsKey = KeyExists(node, key);

		// now add the KV
		node.Add(key, val);

		// we don't have to consider splitting or merging if the key already existed since nothing was added to the set of keys
		if (containsKey)
		{
			return true;
		}
		
		// we are going to traverse up 
		while (node != null && node.IsFull(Order)) 
		{
			BPlusNode rootN = Split(node);

			// if we traversed up to the root that means there is a new root
			if (rootN != null)
			{
				this._root = rootN;
				return true;
			}

			node = node.Parent;

		}

		// merge detached node that was split

		return true;
	}

	public BPlusNode? Split(BPlusNode node)
	{
		// the point at which we divide
		int divider = (int) Math.Floor( (float) (this.Order / 2) );

		BPlusNode lNode = null;
		BPlusNode rNode = null;

		// parent key is the left most key of the right node
		int keyToAdd = node.Keys[divider];

		if (node.NType == NodeType.LEAF)
		{
			SplitLeaf(node, divider, out lNode, out rNode);
		} else
		{
			SplitInternal(node, divider, out lNode, out rNode);
		}

        if (lNode == null || rNode == null)
        {
			return null;
        }

        // if node has a parent
        if (node.Parent != null)
		{
			BPlusNode parent = node.Parent;

			// find where the new parent would reside
			// FindChildIdx takes in the node and uses key passed in to see where it fits in parent
			int index = FindChildIdx(parent, keyToAdd);

			parent.Keys.Insert(index, keyToAdd);

			// note: in bplus tree if a node has n keys then it has n+1 children: given it's an internal node
			if (index > 0)
			{
				// we need to replace here
				parent.Children[index] = lNode;
				// we need to insert here
				parent.Children.Insert(index+1, rNode);
			}
			else if (index == 0)
			{
				// both have to be replaced here since we are changing the children at the beginning of the list
				parent.Children[0] = lNode;
				parent.Children.Insert(1, rNode);
			}

			lNode.Parent = parent;
			rNode.Parent = parent;
		} else
		{
			// node does not have parent then we are splitting a root node
			BPlusNode newRoot = new BPlusNode() { 
				NType = NodeType.INTERNAL,
				Keys = new List<int>() { keyToAdd },
				Children = new List<BPlusNode>() { lNode, rNode }
			};
			lNode.Parent = newRoot;
			rNode.Parent = newRoot;

			return newRoot;
		}

		return null;
	}

	/// <summary>
	/// Splits a leaf node. The main action here is dividing the KEYS and VALUES of some node. It then returns a left node and right node containing the divided contents.
	/// </summary>
	/// <param name="node"></param>
	/// <param name="divider"></param>
	/// <param name="lNode"></param>
	/// <param name="rNode"></param>
	public void SplitLeaf(BPlusNode node, int divider, out BPlusNode lNode, out BPlusNode rNode)
	{
		lNode = new BPlusNode() { NType = NodeType.LEAF };
		rNode = new BPlusNode() { NType = NodeType.LEAF };

		lNode.Keys = node.Keys.ToArray()[0..divider].ToList();
		lNode.Vals = node.Vals.ToArray()[0..divider].ToList();

		rNode.Keys = node.Keys.ToArray()[divider..node.Keys.Count].ToList();
		rNode.Vals = node.Vals.ToArray()[divider..node.Vals.Count].ToList();

		lNode.Next = rNode;
		lNode.Prev = node.Prev;
		rNode.Prev = lNode;
		rNode.Next = node.Next;

		// basically if the next is defined for the right node, then we have to update the next's prev pointer
		if (rNode.Next != null)
		{
			rNode.Next.Prev = rNode;
		}

		if (lNode.Prev != null)
		{
			lNode.Prev.Next = lNode;
		}
	}

	/// <summary>
	/// Splits an internal node. The main action here is dividing the KEYS and CHILDREN of some node. It then returns a left node and right node containing the divided contents.
	/// </summary>
	/// <param name="node"></param>
	/// <param name="divider"></param>
	/// <param name="lNode"></param>
	/// <param name="rNode"></param>
	public void SplitInternal(BPlusNode node, int divider, out BPlusNode lNode, out BPlusNode rNode)
	{

		lNode = new BPlusNode() { NType = NodeType.INTERNAL };
		rNode = new BPlusNode() { NType = NodeType.INTERNAL };

		// minus one because we exlcude the key at the divider
		lNode.Keys = node.Keys.ToArray()[0..(divider)].ToList();
		lNode.Children = node.Children.ToArray()[0..(divider+1)].ToList();
		UpdateParent(lNode.Children, lNode);

		// similarly we exlcude the the key at the divider index
		rNode.Keys = node.Keys.ToArray()[(divider+1)..node.Keys.Count].ToList();
		rNode.Children = node.Children.ToArray()[(divider+1)..node.Children.Count].ToList();
		UpdateParent(rNode.Children, rNode);
		

		lNode.Next = rNode;
		lNode.Prev = node.Prev;
		rNode.Prev = lNode;
		rNode.Next = node.Next;

		// basically if the next is defined for the right node, then we have to update the next's prev pointer
		if (rNode.Next != null)
		{
			rNode.Next.Prev = rNode;
		}

		if (lNode.Prev != null)
		{
			lNode.Prev.Next = lNode;
		}
	}

	// util for updating parent references when splitting internal nodes
	public void UpdateParent(IList<BPlusNode> children, BPlusNode parent)
	{
		foreach(var child in children)
		{
			child.Parent = parent;
		}
	}

	public BPlusNode Find(int key)
	{
		BPlusNode node = _root;

		// Go until we reach a leaf
		// at this point we are traversing through "internal nodes"
        while (node.NType == NodeType.INTERNAL)
        {
			// we find apropriate index of child to descend to
			int indexInsert = FindChildIdx(node, key);

			node = node.Children[indexInsert];
        }

		return node;
    }

	public int FindChildIdx(BPlusNode node, int key)
	{
		int idx = 0;
		foreach(int nKey in node.Keys)
		{
			if (key <= nKey)
			{
				return idx;
			}
			idx++;
		}

		// if we exhauseted everything then the key should simply be appended to the end of the list of keys
		return node.Keys.Count;
	}


	/// <summary>
	/// Binary search on the lsit of keys to check if a key exist as opposed to using a O(n) complexity that .Contains method produces
	/// </summary>
	/// <param name="node"></param>
	/// <param name="key"></param>
	/// <returns></returns>
	public bool KeyExists(BPlusNode node, int key)
	{
		var keys = node.Keys;
		int l = 0;
		int r = keys.Count - 1;
		int m;

		while (l <= r)
		{
			m = (l + r) / 2;

			if (key == keys[m])
			{
				return true;
			}
			else if (key < keys[m])
			{
				r = m - 1;
				continue;
			}
			l = m + 1;
		}

		return false;
	}


	public String PrintTree()
	{
		StringBuilder treeStrBuilder = new StringBuilder();
		PrintHelper(_root, treeStrBuilder);

		Console.WriteLine(treeStrBuilder.ToString());
		return treeStrBuilder.ToString();
	}

	public void PrintHelper(BPlusNode n, StringBuilder builder, int counter = 0)
	{
		builder.Append($"{counter}: ");
		n.Keys.ToList().ForEach(k => builder.Append($"{k},"));
		builder.Append('\n');

		if (n.NType == NodeType.LEAF)
		{
			return;
		}

		foreach (BPlusNode c in n.Children)
		{
			PrintHelper(c, builder, counter + 1);
		}
	}

	public BPlusNode GetRoot()
	{
		return _root;
	}
	
}

public enum NodeType
{
	INTERNAL,
	LEAF
}

public class BPlusNode
{
	public const int LEFT_CHILD = 0;
	public const int RIGHT_CHILD = 1;

	// reference to it's parent so we can traverse up
	public BPlusNode? Parent { get; set; }

	public BPlusNode? Next { get; set; }

	public BPlusNode? Prev { get; set; }

	public NodeType NType { get; set; } = NodeType.LEAF;

	public IList<int>? Keys { get; set; } = new List<int>();

	public IList<int>? Vals { get; set; } = new List<int>();

	public IList<BPlusNode>? Children { get; set; }

	public void Add(int key, int val)
	{
		int index = 0;
		foreach(int currKey in this.Keys)
		{
			// if key already exist simply add the value
			if (key == currKey)
			{
				// if the value already exist we override it for this implementation of bplus key
				this.Vals[index] = val;
				return;
			} 
			// if key is less then some existing key add that new KV
			else if (key < currKey) 
			{
				this.Keys.Insert(index, key);
				this.Vals.Insert(index, val);
				return;
			}

			index++;
		}

		// if we exhausted the list, then simply add the new KV to the end
		if ((index) == this.Keys.Count)
		{
			this.Keys.Add(key);
			this.Vals.Add(val);
		}
	}

	public bool IsFull(int order)
	{
		return order == this.Keys.Count;
	}
}



