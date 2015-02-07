using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using PHP.Core.Utilities;

namespace PHP.Core
{
    /// <summary>
    /// Manages active <see cref="PhpResource"/> instances across the current thread.
    /// </summary>
    internal sealed class PhpResourceManager
    {
        #region Fields & Properties

        /// <summary>
        /// Lazily initialized list of <see cref="PhpResource"/>s created during this web request.
        /// </summary>
        /// <remarks>
        /// The resources are disposed of when the request is over.
        /// <seealso cref="RegisterResource"/><seealso cref="CleanUpResources"/>
        /// </remarks>
        private static RequestStatic<LinkedList<WeakReference>> _resources = new RequestStatic<LinkedList<WeakReference>>(() => _resources.Value);

        #endregion

        #region Construction

        static PhpResourceManager()
        {
            // Registers the cleanup function to the request end event,
            // so any pending resources will be automatically closed.
            RequestContext.RequestEnd += CleanUpResources;
        }

        #endregion

        #region RegisterResource, UnregisterResource, CleanupResources

        /// <summary>
        /// Registers a resource that should be disposed of when the request is over.
        /// </summary>
        /// <param name="res">The resource.</param>
        internal static LinkedListNode<WeakReference> RegisterResource(PhpResource/*!*/res)
        {
            Debug.Assert(res != null);
            //Debug.Assert(this method can only be called on the request thread)

            var resources = _resources.Value;
            if (resources == null)
                resources = _resources.Value = new LinkedList<WeakReference>();

            return resources.AddFirst(new WeakReference(res));
        }

        /// <summary>
        /// Unregisters disposed resource.
        /// </summary>
        internal static void UnregisterResource(LinkedListNode<WeakReference>/*!*/node)
        {
            Debug.Assert(node != null);
            
            node.List.Remove(node); // node.list == resources
        }

        /// <summary>
        /// Disposes of <see cref="PhpResource"/>s created during this web request.
        /// </summary>
        internal static void CleanUpResources()
        {
            var resources = _resources.Value;
            if (resources != null)
            {
                for (var p = resources.First; p != null; )
                {
                    var next = p.Next;
                    if (p.Value.IsAlive)
                    {
                        var phpresource = (PhpResource)p.Value.Target;
                        if (phpresource != null)
                            phpresource.Close();
                    }
                    p = next;
                }

                _resources.Value = null;
            }
        }

        #endregion
    }
}
