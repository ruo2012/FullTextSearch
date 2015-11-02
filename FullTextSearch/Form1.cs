﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Npgsql;

namespace FullTextSearch
{
    public partial class Form1 : Form
    {
        private PostgreSQL pg = new PostgreSQL();
        private SQLquerys sqlQuerys = new SQLquerys();
        private char odabirAndOr;
        private char vrstaPretrazivanja;
        private DataSet dataSet = new DataSet();
        private DataTable dataTable = new DataTable();        
        private Parser parser = new Parser();       

        public Form1()
        {
            InitializeComponent();
            rbtn_AND.Checked = true;
            rbtnNeizmjenjeni.Checked = true;
            odabirAndOr = '*';
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button_unosTekstaUBazu_Click(object sender, EventArgs e)
        {
            string searchTextBoxString = rTB_unosTextaUBazu.Text.ToString();

            if(searchTextBoxString != "")
            {
                pg.insertIntoTable(searchTextBoxString, pg.conn);
                MessageBox.Show(searchTextBoxString + " dodano u bazu podataka!");
                rTB_unosTextaUBazu.Clear();                
            }
            else
            {
                MessageBox.Show("Prazan unos nije moguć! Potrebno je upisati tekst!");
            }
        }

        private void button_Pretrazi_Click(object sender, EventArgs e)
        {
            string stringToSearch;
            string sql;
            string highlitedText;
            string rank;

            stringToSearch = textBox_Pretrazivanje.Text.Trim();
            List<string> list = parser.parseInput(stringToSearch);

            sql = sqlQuerys.createSqlString(list, odabirAndOr, vrstaPretrazivanja);
            richTextBox1.Text = sql;

            pg.conn.Open();
            NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(sql, pg.conn);
            dataSet.Reset();
            dataAdapter.Fill(dataSet);
            pg.conn.Close();

            using (pg.conn)
            {
                pg.conn.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(sql, pg.conn))
                {
                    NpgsqlDataReader reader = command.ExecuteReader();
                    int count = 0;
                    linkLabel_Rezultat.Text = " ";
                    while (reader.Read())
                    {
                        highlitedText = reader[1].ToString() as string;
                        rank = reader[3].ToString() as string;
                        linkLabel_Rezultat.Text += highlitedText + "[" + rank + "]\n";
                        count++;
                    }
                    labelBrojac.Text = "Broj pronađenih dokumenata je: " + count.ToString();
                }
                pg.conn.Close();
            }
        }

        private void rbtn_AND_CheckedChanged(object sender, EventArgs e)
        {
            odabirAndOr = '*';
        }

        private void rbtn_OR_CheckedChanged(object sender, EventArgs e)
        {
            odabirAndOr = '+';
        }

        private void rbtnNeizmjenjeni_CheckedChanged(object sender, EventArgs e)
        {
            vrstaPretrazivanja = 'A';
        }

        private void rbtn_Rijecnici_CheckedChanged(object sender, EventArgs e)
        {
            vrstaPretrazivanja = 'B';
        }

        private void rbtn_Fuzzy_CheckedChanged(object sender, EventArgs e)
        {
            vrstaPretrazivanja = 'C';
        }
    }
}