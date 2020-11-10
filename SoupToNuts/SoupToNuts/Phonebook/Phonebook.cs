using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;

namespace SoupToNuts.Phonebook
{
    public class PhonebookUpdateEventArgs : EventArgs
    {
        public ushort Index { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
    }

    public class PhonebookPageEventArgs : EventArgs
    {
        public ushort Page { get; set; }
        public string[] Names { get; set; }
    }

    public class Phonebook
    {
        public delegate void StatusFeedback (ushort success);
        public delegate void SelectionFeedback (ushort value);
        public delegate void PhonebookUpdateEventHandler (object sender, PhonebookUpdateEventArgs args);
        public delegate void PhonebookPageEventHandler (object sender, PhonebookPageEventArgs args);

        private List<PhonebookEntry> _entries;
        private string _filename;

        public StatusFeedback OnInitialize { get; set; }
        public StatusFeedback OnSave { get; set; }
        public SelectionFeedback OnSelection { get; set; }

        public event PhonebookUpdateEventHandler PhonebookUpdated;
        public event PhonebookPageEventHandler PageUpdated;

        private const ushort MAX_PAGE_SIZE = 500;
        private ushort _pageSize;

        public ushort PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                if ((value > 0) &&
                    (value <= MAX_PAGE_SIZE))
                {
                    _pageSize = value;
                }
            }
        }

        public ushort TotalPages
        {
            get
            {
                ushort pages;

                pages = (ushort)(_entries.Count / PageSize);

                if (_entries.Count % PageSize > 0)
                    pages++;

                if (pages == 0) pages = 1;

                return pages;
            }
        }

        private ushort _currentPage;

        public ushort CurrentPage
        {
            get
            {
                return _currentPage;
            }
            set
            {
                if ((value > 0) &&
                    (value <= TotalPages))
                {
                    _currentPage = value;
                    Selection = 0;

                    if (PageUpdated != null)
                    {
                        var args = new PhonebookPageEventArgs();
                        args.Page = _currentPage;
                        args.Names = new string[_pageSize];

                        for (int i = 0; i < _pageSize; i++)
                        {
                            int j = (_currentPage - 1) * _pageSize + i;

                            if (j < _entries.Count)
                                args.Names[i] = _entries[j].Name;
                            else
                                args.Names[i] = "";
                        }

                        PageUpdated(this, args);
                    }
                }
            }
        }

        private int _selection;

        public ushort Selection
        {
            get
            {
                if (_selection < 0)
                    return 0;
                else
                    return (ushort)(_selection + 1);
            }
            set
            {
                if (value <= _entries.Count)
                {
                    _selection = value - 1;

                    if (_selection < 0)
                    {
                        SelectedEntryName = "";
                        SelectedEntryNumber = "";
                    }
                    else
                    {
                        SelectedEntryName = _entries[_selection].Name;
                        SelectedEntryNumber = _entries[_selection].Number;
                    }

                    if (OnSelection != null) OnSelection((ushort)(_selection + 1));
                }
            }
        }

        public ushort SelectPageEntry
        {
            get
            {
                if (_selection < 0)
                    return 0;

                return (ushort)((_selection + 1) % PageSize);
            }
            set
            {
                if ((value > 0) &&
                    (value <= PageSize))
                {
                    Selection = (ushort)((CurrentPage - 1) * PageSize + value);
                }
            }
        }

        public string SelectedEntryName { get; private set; }
        public string SelectedEntryNumber { get; private set; }

        public Phonebook()
        {
            _entries = new List<PhonebookEntry>();
        }

        public void Initialize(string filename)
        {
            _filename = filename;
            
            _entries.Clear();

            try
            {
                using (var stream = File.OpenText(_filename))
                {
                    while (!stream.EndOfStream)
                    {
                        var text = stream.ReadLine();

                        if (text.IndexOf('|') > 0)
                        {
                            var fields = text.Split('|');
                            _entries.Add(new PhonebookEntry {
                                Name = fields[0],
                                Number = fields[1]
                            });
                        }
                    }
                }

                if (OnInitialize != null) OnInitialize(1);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Exception in Phonebook.Initialize: {0}",
                    e.Message);

                if (OnInitialize != null) OnInitialize(0);
            }
        }

        public void Save()
        {
            try
            {
                using (var stream = File.CreateText(_filename))
                {
                    foreach (var entry in _entries)
                    {
                        stream.WriteLine("{0}|{1}", entry.Name, entry.Number);
                    }
                }

                if (OnSave != null) OnSave(1);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Exception in Phonebook.Save: {0}",
                    e.Message);

                if (OnSave != null) OnSave(0);
            }
        }

        public void Add(string name, string number)
        {
            _entries.Add(new PhonebookEntry { Name = name, Number = number });

            if (PhonebookUpdated != null)
            {
                PhonebookUpdated(this, new PhonebookUpdateEventArgs {
                    Index = (ushort)_entries.Count,
                    Name = name,
                    Number = number });
            }
        }

        public void Remove(ushort index)
        {
            if ((index > 0) &&
                (index <= _entries.Count))
            {
                _entries.RemoveAt(index - 1);

                if (PhonebookUpdated != null)
                {
                    PhonebookUpdated(this, new PhonebookUpdateEventArgs {
                        Index = index,
                        Name = "",
                        Number = "" });            
                }
            }
        }
    }
}