using Anamnesis.Core.Domains;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Anamnesis.Core.Data
{
    public static class CRUD
    {
        private const string AUTHORS_FILENAME = "SelectedChangesetAuthors.xml";

        public static bool UpdateSelectedChangesetAuthors(IEnumerable<string> selectedChangesetAuthors, ILogger logger)
        {
            try
            {
                logger.Information("Saving selected changeset authors.");

                StringBuilder xml = CreateBaseXMLStringBuilder();
                xml.AppendLine("<SelectedChangesetAuthors>");

                foreach (string author in selectedChangesetAuthors)
                {
                    xml.AppendLine($"<SelectedChangesetAuthor>{author}</SelectedChangesetAuthor>");
                }

                xml.AppendLine("</SelectedChangesetAuthors>");

                CreateXMLFile(xml.ToString(), GetSelectedChangesetAuthorsFilePath());

                return true;
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                return false;
            }
        }

        public static IEnumerable<string> ReadSelectedChangesetAuthors(List<string> authorizedUsers, ILogger logger)
        {
            try
            {
                logger.Information("Reading saved selected changeset authors.");

                string xmlFilePath = GetSelectedChangesetAuthorsFilePath();

                if (File.Exists(xmlFilePath))
                {
                    XDocument xml = new XDocument();
                    xml = XDocument.Parse(GetResourceText(xmlFilePath));

                    return xml.Element("SelectedChangesetAuthors")
                        .Descendants()
                        .Select(sca => sca.Value)
                        .Where(sca => authorizedUsers.Contains(sca)) // In case someone manually altered the XML file with a name that is not authorized.
                        .OrderBy(sca => sca);
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }

            return new List<string>();
        }

        public static IEnumerable<Changeset> ReadTFSChangesets(VersionControlServer versionControlServer, ChangesetQuery changesetQuery, string branchMapping, ILogger logger)
        {
            if (versionControlServer == null)
            {
                return new List<Changeset>(0);
            }

            try
            {
                logger.Information("Querying Changesets from TFS.");

                if (!changesetQuery.SelectedChangesetAuthors.Any())
                {
                    QueryHistoryParameters query = new QueryHistoryParameters(branchMapping, RecursionType.Full)
                    {
                        VersionStart = new DateVersionSpec(changesetQuery.LowDate.Value),
                        VersionEnd = new DateVersionSpec(changesetQuery.HighDate.Value)
                    };

                    return versionControlServer
                        .QueryHistory(query)
                        .Select(c => versionControlServer.GetChangeset(c.ChangesetId));
                }
                else
                {
                    List<Changeset> changesets = new List<Changeset>();

                    foreach (string author in changesetQuery.SelectedChangesetAuthors)
                    {
                        QueryHistoryParameters query = new QueryHistoryParameters(branchMapping, RecursionType.Full)
                        {
                            VersionStart = new DateVersionSpec(changesetQuery.LowDate.Value),
                            VersionEnd = new DateVersionSpec(changesetQuery.HighDate.Value),
                            Author = author
                        };

                        changesets.AddRange(versionControlServer
                            .QueryHistory(query)
                            .Select(c => versionControlServer.GetChangeset(c.ChangesetId)));
                    }

                    return changesets.OrderByDescending(c => c.CreationDate);
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                return null;
            }
        }

        public static IEnumerable<TeamFoundationIdentity> ReadTFSUsers(TfsTeamProjectCollection tfsTeamProjectCollection, ILogger logger)
        {
            try
            {
                logger.Information("Querying Team Foundation Identities from TFS.");

                IIdentityManagementService identityService = tfsTeamProjectCollection.GetService<IIdentityManagementService>();

                TeamFoundationIdentity teamFoundationIdentity = identityService.ReadIdentity(IdentitySearchFactor.AccountName, "Project Collection Valid Users", MembershipQuery.Expanded, ReadIdentityOptions.ExtendedProperties);

                TeamFoundationIdentity[][] teamFoundationIdentities = identityService
                    .ReadIdentities(
                        IdentitySearchFactor.Identifier,
                        teamFoundationIdentity
                            .Members
                            .Select(id => id.Identifier)
                            .ToArray(),
                        MembershipQuery.None,
                        ReadIdentityOptions.ExtendedProperties);

                return teamFoundationIdentities
                    .SelectMany(tfi => tfi)
                    .Where(tfi => !tfi.IsContainer);
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                return null;
            }
        }

        private static string GetSelectedChangesetAuthorsFilePath()
        {
            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                AUTHORS_FILENAME);
        }

        private static string GetEmbeddedResourceText(string filename)
        {
            string result = string.Empty;

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(filename))
            using (StreamReader streamReader = new StreamReader(stream))
            {
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        private static string GetResourceText(string filePath)
        {
            string result = string.Empty;

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (StreamReader streamReader = new StreamReader(stream))
            {
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        private static StringBuilder CreateBaseXMLStringBuilder()
        {
            StringBuilder baseDocument = new StringBuilder();
            baseDocument.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");

            return baseDocument;
        }

        private static void CreateXMLFile(string xml, string filePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            XmlTextWriter writer = new XmlTextWriter(filePath, null)
            {
                Formatting = System.Xml.Formatting.Indented
            };

            doc.Save(writer);
        }
    }
}
