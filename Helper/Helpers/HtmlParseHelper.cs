using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;

namespace PublicUtilities
{
    public class HtmlParseHelper
    {
        private readonly object lockOuterTextObject = new object();
        private readonly object lockInnerTextObject = new object();
        private readonly string[] innerTextSplit = { ">", " >", "'>", "' >", "\">", "\" >" };
        private const string HTMLPREFIX = "<";

        public static bool IsContains(string rawContent, params string[] subStrings)
        {
            foreach (string item in subStrings)
            {
                if (rawContent.IndexOf(item.Trim(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get outer text from html
        /// </summary>
        /// <param name="startSymbol"></param>
        /// <param name="endSymbol"></param>
        /// <param name="index">Get outer text from html</param>
        /// <param name="foundIndex">the real found value index</param>
        /// <param name="rawContent"></param>
        /// <returns></returns>
        public string GetOuterTextFromHtml(string startSymbol, string endSymbol, int index, out int foundIndex, string rawContent)
        {
            lock (lockOuterTextObject)
            {
                int startIndex = -1;
                int endIndex = -1;
                string subConten = string.Empty;
                foundIndex = 0;
                while (true)
                {
                    startIndex = rawContent.IndexOf(startSymbol, 0, StringComparison.CurrentCultureIgnoreCase);
                    startIndex = startIndex < 0 ? 0 : startIndex;
                    endIndex = rawContent.IndexOf(endSymbol, startIndex, StringComparison.CurrentCultureIgnoreCase);
                    if ((startIndex < 0) || (endIndex < 0))
                    {
                        return subConten;
                    }

                    subConten = rawContent.Substring(startIndex, endIndex - startIndex + endSymbol.Length);
                    rawContent = rawContent.Remove(startIndex, endIndex - startIndex + endSymbol.Length);

                    if (++foundIndex == index)
                    {
                        return subConten;
                    }
                }
            }
        }

        /// <summary>
        /// Get outer text from html
        /// </summary>
        /// <param name="startSymbol"></param>
        /// <param name="endSymbol"></param>
        /// <param name="index">start from 1</param>
        /// <param name="rawContent"></param>
        /// <returns></returns>
        public string GetOuterTextFromHtml(string startSymbol, string endSymbol, int index, string rawContent)
        {
            lock (lockOuterTextObject)
            {
                int foundIndex = 0;
                return GetOuterTextFromHtml(startSymbol, endSymbol, index, out foundIndex, rawContent);
            }
        }

        /// <summary>
        /// Get all outer text from html
        /// </summary>
        /// <param name="startSymbol"></param>
        /// <param name="endSymbol"></param>
        /// <param name="rawContent"></param>
        /// <returns></returns>
        public IList<string> GetOuterTextFromHtml(string startSymbol, string endSymbol, string rawContent)
        {
            lock (lockOuterTextObject)
            {
                IList<string> itemList = new List<string>();
                while (rawContent.IndexOf(startSymbol) != -1)
                {
                    string item = this.GetOuterTextFromHtml(startSymbol, endSymbol, 1, rawContent);
                    if (!string.IsNullOrEmpty(item))
                    {
                        rawContent = rawContent.Replace(item, "");
                        itemList.Add(item);
                    }
                }
                return itemList;
            }
        }

        /// <summary>
        /// Get all outer text from html, and text contains containSymbol
        /// </summary>
        /// <param name="startSymbol"></param>
        /// <param name="endSymbol"></param>
        /// <param name="rawContent"></param>
        /// <returns></returns>
        public IList<string> GetOuterTextFromHtml(string startSymbol, string endSymbol, string containSymbol, string rawContent)
        {
            lock (lockOuterTextObject)
            {
                IList<string> itemList = new List<string>();
                while (rawContent.IndexOf(startSymbol) != -1)
                {
                    string item = this.GetOuterTextFromHtml(startSymbol, endSymbol, 1, rawContent);
                    if (!string.IsNullOrEmpty(item))
                    {
                        rawContent = rawContent.Replace(item, "");
                        if (TextHelper.IsContains(item, containSymbol))
                        {
                            itemList.Add(item);
                        }
                    }
                }
                return itemList;
            }
        }

        /// <summary>
        /// Get outer text from html
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="startSymbol"></param>
        /// <param name="endSymbol"></param>
        /// <param name="index">start from 1</param>
        /// <param name="rawContent"></param>
        /// <returns></returns>
        public string GetOuterTextFromHtml(int startIndex, string startSymbol, string endSymbol, int index, string rawContent)
        {
            lock (lockOuterTextObject)
            {
                if ((startIndex >= 0) && (startIndex < rawContent.Length))
                {
                    int foundIndex = 0;
                    return GetOuterTextFromHtml(startSymbol, endSymbol, index, out foundIndex,
                                                rawContent.Substring(startIndex, rawContent.Length - startIndex));
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Get outer text from html
        /// </summary>
        /// <param name="identifySymbol">The symbol to find the start index</param>
        /// <param name="startSymbol"></param>
        /// <param name="endSymbol"></param>
        /// <param name="index">start from 1</param>
        /// <param name="rawContent"></param>
        /// <returns></returns>
        public string GetOuterTextFromHtml(string identifySymbol, string startSymbol, string endSymbol, int index, string rawContent)
        {
            lock (lockOuterTextObject)
            {
                int startIndex = rawContent.IndexOf(identifySymbol, StringComparison.CurrentCultureIgnoreCase);
                if ((startIndex < 0) || (startIndex >= rawContent.Length))
                {
                    return string.Empty;
                }

                return GetOuterTextFromHtml(startIndex, startSymbol, endSymbol, index, rawContent);
            }
        }

        /// <summary>
        /// Get all inner text from html
        /// </summary>
        /// <param name="startSymbol"></param>
        /// <param name="endSymbol"></param>
        /// <param name="rawContent"></param>
        /// <returns></returns>
        public IList<string> GetInnerTextListFromHtml(string startSymbol, string endSymbol, string rawContent)
        {
            lock (lockOuterTextObject)
            {
                IList<string> outterItemList = this.GetOuterTextFromHtml(startSymbol, endSymbol, rawContent);
                IList<string> innerItemList = new List<string>();
                if (outterItemList.Count > 0)
                {
                    foreach (string item in outterItemList)
                    {
                        string innerItem = GetInnerTextFromHtml(item);
                        if (!string.IsNullOrEmpty(innerItem))
                        {
                            innerItemList.Add(innerItem);
                        }
                    }
                }

                return innerItemList;
            }
        }

        public string GetInnerTextFromHtml(string startSymbol, string endSymbol, string rawContent)
        {
            lock (lockOuterTextObject)
            {
                IList<string> outterItemList = this.GetOuterTextFromHtml(startSymbol, endSymbol, rawContent);

                if (outterItemList.Count > 0)
                {
                    foreach (string item in outterItemList)
                    {
                        string innerItem = GetInnerTextFromHtml(item);
                        if (!string.IsNullOrEmpty(innerItem))
                        {
                            return innerItem;
                        }
                    }
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Get all inner text from html, and the outter text contains outterTextContainSymbol
        /// </summary>
        /// <param name="startSymbol"></param>
        /// <param name="endSymbol"></param>
        ///  <param name="endSymbol"></param>
        /// <param name="rawContent"></param>
        /// <returns></returns>
        public IList<string> GetInnerTextFromHtml(string startSymbol, string endSymbol, string outterTextContainSymbol, string rawContent)
        {
            lock (lockOuterTextObject)
            {
                IList<string> outterItemList = this.GetOuterTextFromHtml(startSymbol, endSymbol, outterTextContainSymbol, rawContent);
                IList<string> innerItemList = new List<string>();
                if (outterItemList.Count > 0)
                {
                    foreach (string item in outterItemList)
                    {
                        string innerItem = GetInnerTextFromHtml(item);
                        if (!string.IsNullOrEmpty(innerItem))
                        {
                            innerItemList.Add(innerItem);
                        }
                    }
                }

                return innerItemList;
            }
        }

        /// <summary>
        /// Get inner text from html
        /// </summary>
        /// <param name="rawContent"></param>
        /// <returns></returns>
        public string GetInnerTextFromHtml(string rawContent)
        {
            lock (lockInnerTextObject)
            {
                return GetInnerTextFromHtml(rawContent, 0);
            }
        }

        /// <summary>
        /// Get property value from html outter text
        /// </summary>
        /// <param name="rawContent"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string GetOutterPropertyFromHtml(string rawContent, string propertyName)
        {
            lock (lockInnerTextObject)
            {
                int index = rawContent.IndexOf(propertyName, StringComparison.CurrentCultureIgnoreCase);
                if (index >= 0)
                {
                    //" or ' all may be the value split char
                    string tempContent = rawContent.Substring(index);
                    int valueStartIndex = tempContent.IndexOf("\"");
                    if (valueStartIndex < 0)
                    {
                        valueStartIndex = tempContent.IndexOf("\'");
                    }

                    if ((valueStartIndex >= 0) && (valueStartIndex < tempContent.Length - 1))
                    {
                        // from the char after first "
                        tempContent = tempContent.Substring(valueStartIndex + 1);
                        int valueEndIndex = tempContent.IndexOf("\"");
                        if (valueEndIndex < 0)
                        {
                            valueEndIndex = tempContent.IndexOf("\'");
                        }

                        if (valueEndIndex > 0)
                        {
                            return tempContent.Substring(0, valueEndIndex);
                        }
                    }
                }

                return string.Empty;
            }
        }

        public string GetOutterPropertyFromHtml(string rawContent, string propertyName, string propertyValueContainer)
        {
            lock (lockInnerTextObject)
            {
                int index = rawContent.IndexOf(propertyName, StringComparison.CurrentCultureIgnoreCase);
                if (index >= 0)
                {
                    //" or ' all may be the value split char
                    string tempContent = rawContent.Substring(index);
                    int valueStartIndex = tempContent.IndexOf(propertyValueContainer);

                    if ((valueStartIndex >= 0) && (valueStartIndex < tempContent.Length - 1))
                    {
                        // from the char after first "
                        tempContent = tempContent.Substring(valueStartIndex + 1);
                        int valueEndIndex = tempContent.IndexOf(propertyValueContainer);

                        if (valueEndIndex > 0)
                        {
                            return tempContent.Substring(0, valueEndIndex);
                        }
                    }
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Get property value from html outter text
        /// </summary>
        /// <param name="startSymbol"></param>
        /// <param name="endSymbol"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string GetPropertyFromHtml(string startSymbol, string endSymbol, string propertyName, string rawContent)
        {
            lock (lockOuterTextObject)
            {
                IList<string> outterItemList = this.GetOuterTextFromHtml(startSymbol, endSymbol, rawContent);
                string popValue = string.Empty;
                if (outterItemList.Count > 0)
                {
                    foreach (string item in outterItemList)
                    {
                        string popItem = GetOutterPropertyFromHtml(item, propertyName);
                        if (!string.IsNullOrEmpty(popItem))
                        {
                            popValue = popItem;
                            break;
                        }
                    }
                }
                return popValue;
            }
        }

        /// <summary>
        /// Get property value from html outter text
        /// </summary>
        /// <param name="startSymbol"></param>
        /// <param name="endSymbol"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public IList<string> GetOutterPropertyFromHtml(string startSymbol, string endSymbol, string propertyName, string rawContent)
        {
            lock (lockOuterTextObject)
            {
                IList<string> outterItemList = this.GetOuterTextFromHtml(startSymbol, endSymbol, rawContent);
                IList<string> popList = new List<string>();
                if (outterItemList.Count > 0)
                {
                    foreach (string item in outterItemList)
                    {
                        string popItem = GetOutterPropertyFromHtml(item, propertyName);
                        if (!string.IsNullOrEmpty(popItem))
                        {
                            popList.Add(popItem);
                        }
                    }
                }

                return popList;
            }
        }

        /// <summary>
        /// Get inner text from html
        /// </summary>
        /// <param name="rawContent"></param>
        /// <param name="rowItems">how many items to be a row,if(<=0) is only a row</param>
        /// <returns></returns>
        public string GetInnerTextFromHtml(string rawContent, int rowItems)
        {
            lock (lockInnerTextObject)
            {
                StringBuilder sb = new StringBuilder();
                int i = 0;
                string content = string.Empty;

                string[] list = rawContent.Split(this.innerTextSplit, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in list)
                {
                    if (!s.Trim().StartsWith(HTMLPREFIX))
                    {
                        i++;
                        int index = s.IndexOf(HTMLPREFIX);
                        if (-1 == index)
                        {
                            continue;
                        }
                        content = string.Format("{0}  ", s.Substring(0, index).Trim());
                        if (rowItems == 0)
                        {
                            sb.Append(content);
                            continue;
                        }

                        if (i % rowItems == 0)
                        {
                            sb.AppendLine(content);
                        }
                        else
                        {
                            sb.Append(content);
                        }
                    }
                }

                return sb.ToString();
            }
        }

        public string GetSubString(string rawString, string startLabel, string endLable)
        {
            if (string.IsNullOrEmpty(rawString))
            {
                return rawString;
            }

            int index = rawString.IndexOf(startLabel);
            if (index > 0 && index + startLabel.Length < rawString.Length)
            {
                rawString = rawString.Substring(index + startLabel.Length);
                index = rawString.IndexOf(endLable);
                if (index > 0 && index < rawString.Length)
                {
                    string value = rawString.Substring(0, index);
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value;
                    }
                }
            }
            return string.Empty;
        }

        public string GetSubString(string rawString, string startLabel, string endLable, string defaultValue)
        {
            if (string.IsNullOrEmpty(rawString))
            {
                return defaultValue;
            }

            int index = rawString.IndexOf(startLabel);
            if (index > 0 && index + startLabel.Length < rawString.Length)
            {
                rawString = rawString.Substring(index + startLabel.Length);
                index = rawString.IndexOf(endLable);
                if (index > 0 && index < rawString.Length)
                {
                    string value = rawString.Substring(0, index);
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value;
                    }
                }
            }
            return defaultValue;
        }

        public void Test_GetOuterTextFromHtml()
        {
            string content = "<table>" +
                "<tr>  <td class=\"left-col\">License Status:</td> <td class=\"right-col\"><span class=\'status-active\'>Active (Expires: Jul 4, 2010)</span></td> </tr>" +
                "<tr>  <td class=\"left-col\">Account Level:</td> <td class=\"right-col\">Wrath of the Lich King</td> </tr> " +
                "<tr><td class=\"left-col\">WoW Remote:</td><td class=\"right-col\"><span class='status-unsubscribed'>Unsubscribed</span></td> </tr></table>";

            int index = 0;
            string subContent = this.GetOuterTextFromHtml("<tr>", "</tr>", 2, out index, content);
            Trace.Assert(subContent == "<tr>  <td class=\"left-col\">Account Level:</td> <td class=\"right-col\">Wrath of the Lich King</td> </tr>", "Get bad sub content");

            string innerText = GetInnerTextFromHtml(subContent);
            Trace.Assert(innerText == "Account Level:\tWrath of the Lich King\t", "Get bad inner content");

            CommentAttributeGetter.GetAttribute<GameServerType>(GameServerType.EUBattle);
        }

        public string Replace(string rawContent, string startSymbol, string endSymbol, string newSubContent)
        {
            string subCotent = GetOuterTextFromHtml(startSymbol, endSymbol, 1, rawContent);
            rawContent = rawContent.Replace(subCotent, newSubContent);
            return rawContent;
        }
    }
}
