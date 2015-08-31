/*

 Copyright (c) 2015 Kendall Bennett
  
 The use and distribution terms for this software are contained in the file named License.txt, 
 which can be found in the root of the Phalanger distribution. By using this software 
 in any fashion, you are agreeing to be bound by the terms of this license.
 
 You must not remove this notice from this software.

*/

using System;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;

namespace PHP.Core
{
    /// <summary>
    /// Global property collection to maintain thread static variables
    /// </summary>
    public static class ThreadStatic
    {
        /// <summary>
        /// Call context name for the current <see cref="ScriptContext"/>.
        /// </summary>
        private const string callContextSlotName = "PhpNet:ThreadStatic";

        /// <summary>
        /// Thread local properties collection
        /// </summary>
        [DebuggerNonUserCode]
        public static PropertyCollectionClass /*!*/ Properties
        {
            [Emitted]
            get
            {
                try
                {
                    var properties = ((PropertyCollectionClass) CallContext.GetData(callContextSlotName));
                    if (properties == null)
                    {
                        properties = new PropertyCollectionClass();
                        CallContext.SetData(callContextSlotName, properties);
                    }
                    return properties;
                }
                catch (InvalidCastException)
                {
                    throw new InvalidCallContextDataException(callContextSlotName);
                }
            }
            set
            {
                if (value == null)
                    CallContext.FreeNamedDataSlot(callContextSlotName);
                else
                    CallContext.SetData(callContextSlotName, value);
            }

            // TODO! Make sure this gets cleared when the script context goes away ...
        }
    }
}
