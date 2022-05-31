﻿using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TestesMariana.Dominio.ModuloDisciplina;
using TestesMariana.Dominio.ModuloMateria;
using TestesMariana.Dominio.ModuloQuestao;
using TestesMariana.Infra.Arquivos.ModuloDisciplina;
using TestesMariana.Infra.Arquivos.ModuloMateria;
using TestesMariana.Infra.BancoDeDados;

namespace TestesMariana.WinApp.ModuloQuestao
{
    public partial class TelaCadastroQuestaoForm : Form
    {
        private Questao _questao;
        private RepositorioDiscplinaEmBancoDeDados _repositorioDisciplina;
        private RepositorioMateriaEmBancoDeDacos _repositorioMateria;

        public Questao Questao
        {
            get
            {
                return _questao;
            }
            set
            {
                _questao = value;
                textBoxNumero.Text = _questao.Numero.ToString();
                comboBoxDisciplinas.SelectedItem = _questao.Disciplina;
                comboBoxMaterias.SelectedItem = _questao.Materia;
                textBoxEnunciado.Text = _questao.Enunciado;
                PovoarAlternativas();
            }
        }

        public TelaCadastroQuestaoForm(RepositorioDiscplinaEmBancoDeDados rd, RepositorioMateriaEmBancoDeDacos rm)
        {
            InitializeComponent();
            this._repositorioDisciplina = rd;
            this._repositorioMateria = rm;
            PovoarDisciplinas();
            comboBoxDisciplinas.SelectedItem = 0;
        }


        public void PovoarDisciplinas()
        {
            List<Disciplina> disciplinas = _repositorioDisciplina.SelecionarTodos();
            foreach (var item in disciplinas)
                comboBoxDisciplinas.Items.Add(item);
        }

        public void PovoarMaterias(Disciplina disc)
        {
            List<Materia> materias = _repositorioMateria.SelecionarTodos();
            List<Materia> materiasEspecificas = new();

            foreach (var item in materias)
                if (item.Disciplina == disc)
                    materiasEspecificas.Add(item);

            foreach (var item in materiasEspecificas)
                comboBoxMaterias.Items.Add(item);
        }

        public void PovoarAlternativas()
        {
            int i = 0;
            foreach (var item in Questao.Alternativas)
            {
                checkedListBoxAlternativas.Items.Add(item);
                if (item.EstaCerta)
                    checkedListBoxAlternativas.SetItemChecked(i, true);

                i++;
            }
        }

        public Func<Questao, ValidationResult>? GravarRegistro { get; set; }

        public List<Alternativa> AlternativasAdicionadas
        {
            get
            {
                return checkedListBoxAlternativas.Items.Cast<Alternativa>().ToList();
            }
        }

        private void buttonGravar_Click(object sender, EventArgs e)
        {
            Questao.Enunciado = textBoxEnunciado.Text.ToString();
            Questao.Disciplina = (Disciplina)comboBoxDisciplinas.SelectedItem;
            Questao.Materia = (Materia)comboBoxMaterias.SelectedItem;

            List<Alternativa> alts = AlternativasAdicionadas;
            foreach (var item in alts)
                if (checkedListBoxAlternativas.SelectedItem == item)
                    item.EstaCerta = true;

            Questao.AdicionarAlternativas(alts);

            var resultadoValidacao = GravarRegistro!(Questao);

            if (!resultadoValidacao.IsValid)
            {
                string erro = resultadoValidacao.Errors[0].ErrorMessage;

                TelaPrincipalForm.Instancia!.AtualizarRodape(erro);

                DialogResult = DialogResult.None;
            }
        }

        private void buttonAdicionar_Click(object sender, EventArgs e)
        {
            if (textBoxAlternativa.Text != string.Empty && textBoxAlternativa.Text != "")
            {
                if (checkedListBoxAlternativas.Items.Count < 4)
                {
                    List<string> titulos = AlternativasAdicionadas.Select(x => x.Opcao).ToList();

                    if (titulos.Count == 0 || titulos.Contains(textBoxAlternativa.Text) == false)
                    {
                        Alternativa alt = new();

                        alt.Opcao = textBoxAlternativa.Text;

                        checkedListBoxAlternativas.Items.Add(alt);
                        textBoxAlternativa.Clear();
                        textBoxAlternativa.Focus();
                    }
                }
            }
        }

        private void checkedListBoxAlternativas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (checkedListBoxAlternativas.CheckedItems.Count > 0)
            {
                foreach (int i in checkedListBoxAlternativas.CheckedIndices)
                    checkedListBoxAlternativas.SetItemCheckState(i, CheckState.Unchecked);

                List<Alternativa> alts = AlternativasAdicionadas;

                foreach (var item in alts)
                    if (checkedListBoxAlternativas.SelectedItem != item)
                        item.EstaCerta = false;
            }
        }

        private void comboBoxDisciplinas_SelectedValueChanged(object sender, EventArgs e)
        {
            comboBoxMaterias.Items.Clear();
            comboBoxMaterias.ResetText();
            PovoarMaterias((Disciplina)comboBoxDisciplinas.SelectedItem);
            comboBoxMaterias.Enabled = true;
        }
    }
}
