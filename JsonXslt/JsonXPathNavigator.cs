using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using Newtonsoft.Json.Linq;

namespace JsonXslt
{
	public class JsonXPathNavigator : XPathNavigator
	{
        private string _rootNodeDefaultName;
		private JToken _currentObject;
		private List<int> _arrayIdxStack = new List<int>();
		private XPathNodeType _type = XPathNodeType.Root;

		public JsonXPathNavigator(JToken json)
		{
            _rootNodeDefaultName = "Document";
			_currentObject = json;
			_arrayIdxStack.Add(-1);
		}

        public JsonXPathNavigator(JToken json, string rootNodeDefaultName)
        {
            _rootNodeDefaultName = rootNodeDefaultName;
            _currentObject = json;
            _arrayIdxStack.Add(-1);
        }

		public override string BaseURI
		{
			get { return string.Empty; }
		}

		public override XPathNavigator Clone()
		{
			return new JsonXPathNavigator(_currentObject, _rootNodeDefaultName)
            {
                _type = _type,
                _arrayIdxStack = new List<int>(_arrayIdxStack)
            };
		}

		public override bool IsEmptyElement
		{
			get
			{
				return _type != XPathNodeType.Root && !_currentObject.HasValues;
			}
		}

		public override bool IsSamePosition(XPathNavigator other)
		{
			JsonXPathNavigator nav = other as JsonXPathNavigator;

			if (nav == null)
			{
				return false;
			}

			return nav._currentObject == _currentObject && nav._type == _type && nav._arrayIdxStack.Count == _arrayIdxStack.Count && nav._arrayIdxStack[nav._arrayIdxStack.Count - 1] == _arrayIdxStack[nav._arrayIdxStack.Count - 1];
		}

		private int CurrentIdx()
		{
			return _arrayIdxStack[_arrayIdxStack.Count - 1];
		}

		private void CurrentIdx(int idx)
		{
			_arrayIdxStack[_arrayIdxStack.Count - 1] = idx;
		}

		private string GetName()
		{
			string path = _currentObject.Path;

			if (string.IsNullOrEmpty(path))
			{
                return _rootNodeDefaultName;
			}
            
			int idx = path.LastIndexOf('.');
            string nodeName = "";

			if (idx == -1)
			{
				nodeName = path;
			}

			nodeName = path.Substring(idx + 1, path.Length - idx - 1);

            if (_currentObject is JArray
                && nodeName[nodeName.Length-1] == 's')
            {
                return nodeName.Substring(0, nodeName.Length - 1);
            }

            return nodeName;
		}

		public override string LocalName
		{
			get
			{
				return GetName();
			}
		}

		public override bool MoveTo(XPathNavigator other)
		{
			JsonXPathNavigator nav = other as JsonXPathNavigator;

			if (nav == null)
			{
				throw new InvalidOperationException();
			}

			_currentObject = nav._currentObject;
			_type = nav._type;
			_arrayIdxStack = new List<int>(nav._arrayIdxStack);

			return true;
		}

		public override bool MoveToFirstAttribute()
		{
			return false;
		}

		public override bool MoveToFirstChild()
		{
			switch (_type)
			{
				case XPathNodeType.Root:
					_type = XPathNodeType.Element;
					return true;
				case XPathNodeType.Text:
					return false;
			}

			JToken obj;

			var jarray = _currentObject as JArray;
			if (jarray != null)
			{
				obj = jarray[CurrentIdx()];
			}
			else
			{
				obj = _currentObject;

				if (obj is JProperty)
				{
					obj = ((JProperty)obj).Value;
				}
				else
				{
					obj = obj.First;
				}
			}

			if (obj is JObject)
			{
				obj = ((JObject)obj).First;
			}

			if (obj is JArray)
			{
				_currentObject = obj;
				_arrayIdxStack.Add(0);

				return true;
			}

			if (obj != null)
			{
				_currentObject = obj;
				_type = _currentObject is JValue ? XPathNodeType.Text : XPathNodeType.Element;
				_arrayIdxStack.Add(-1);
			}

			return obj != null;
		}

		public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
		{
			return false;
		}

		public override bool MoveToId(string id)
		{
			return false;
		}

		public override bool MoveToNext()
		{
			if (_type == XPathNodeType.Root || _type == XPathNodeType.Text)
			{
				return false;
			}

			if (CurrentIdx() != -1)
			{
				if (CurrentIdx() + 1 < ((JArray)_currentObject).Count)
				{
					CurrentIdx(CurrentIdx() + 1);
					return true;
				}

				return false;
			}

			JToken next = _currentObject.Next;

			if (next != null)
			{
				_currentObject = next;
			}

			return next != null;
		}

		public override bool MoveToNextAttribute()
		{
			return false;
		}

		public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
		{
			return false;
		}

		public override bool MoveToParent()
		{
			if (_currentObject is JArray)
			{
				if (CurrentIdx() == -1)
				{
					_arrayIdxStack.RemoveAt(_arrayIdxStack.Count - 1);
					_type = XPathNodeType.Element;
					return true;
				}
			}

			JToken parent = _currentObject.Parent;

			if (parent != null)
			{
				if (parent is JObject && parent.Parent != null)
				{
					parent = parent.Parent;
				}

				_arrayIdxStack.RemoveAt(_arrayIdxStack.Count - 1);
				_currentObject = parent;
				_type = XPathNodeType.Element;
			}
			else
			{
				if (_type == XPathNodeType.Root)
				{
					return false;
				}
				
				_type = XPathNodeType.Root;
				return true;
			}

			return true;
		}

		public override bool MoveToPrevious()
		{
			if (_type == XPathNodeType.Root)
			{
				return false;
			}

			if (CurrentIdx() != -1)
			{
				if (CurrentIdx() - 1 >= 0)
				{
					CurrentIdx(CurrentIdx() - 1);
					return true;
				}

				return false;
			}

			JToken previous = _currentObject.Previous;

			if (previous != null)
			{
				_currentObject = previous;
			}

			return previous != null;
		}

		public override string Name
		{
			get
			{
				return GetName();
			}
		}

		public override XmlNameTable NameTable
		{
			get { return new NameTable(); }
		}

		public override string NamespaceURI
		{
			get { return string.Empty; }
		}

		public override XPathNodeType NodeType
		{
			get
			{
				return _type;
			}
		}

		public override string Prefix
		{
			get
			{
				return string.Empty;
			}
		}

		public override string Value
		{
			get
			{
				if (CurrentIdx() != -1)
				{
					return ((JArray)_currentObject)[CurrentIdx()].ToString();
				}

				if (_currentObject is JProperty)
				{
					return ((JProperty)_currentObject).Value.ToString();
				}
				else
				{
					return _currentObject.ToString();
				}
			}
		}
	}
}
