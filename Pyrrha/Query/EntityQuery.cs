
#region Referencing

using System.Linq;

#endregion

namespace Pyrrha.Util.Query
{
    public static class EntityQuery
    {
        /// <summary>
        ///     query types:
        ///         SELECT, UPDATE, DELETE
        /// 
        ///     Examples: 
        ///         <code>"SELECT * WHERE (color='index' AND layer='name') OR (layer='name' AND linetype='continuous')</code>
        /// 
        /// 
        ///         <code> "SELECT 'INSERT' WHERE (color!='index') AND (tag='tagname' OR value='attributeValue')</code>
        ///         
        ///          <code>"UPDATE ('LINE' OR 'CIRCLE') 
        ///          SET color='index', 
        ///              linetype='continuous' 
        ///          WHERE (color='index' OR color='otherIndex') OR (linetype='hidden')"</code>
        ///                
        ///          <code>"UPDATE 'ALLTEXT'  
        ///          SET value="anotherValue" 
        ///          WHERE value='string'"</code>
        /// 
        ///          <code>"DELETE 'LINE' WHERE (start.x > '10' OR end.y > '4') </code>
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static object ExecuteQuery( this PyrrhaDocument doc, string query )
        {
            return null;
        }  
    }
}
