using QlikView.Qvx.QvxLibrary;

namespace QvFacebookConnector
{
    /// <summary>
    /// The IFacebokTableCreate is an interface which all TableCreator must implement.
    /// </summary>
    public interface IFacebookTableCreator
    {
        /// <summary>
        /// Get the QvxTable.
        /// </summary>
        /// <returns>The QvxTable.</returns>
        QvxTable GetTable();
    }
}
