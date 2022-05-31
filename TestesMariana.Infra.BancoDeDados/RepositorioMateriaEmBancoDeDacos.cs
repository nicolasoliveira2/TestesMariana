using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestesMariana.Dominio.ModuloMateria;

namespace TestesMariana.Infra.BancoDeDados
{
    public class RepositorioMateriaEmBancoDeDacos
    {
        private const string enderecoBanco =
          @"Data Source=(LocalDB)\MSSqlLocalDb;Initial Catalog=MarianaTestsDb;Integrated Security=True;Pooling=False";

        private const string sqlInserir =
            @"INSERT INTO [tb_materia]
                (
                    [NOME],
                    [SERIE],
                    [DISCIPLINA]

                )
                    VALUES
                (
                    @NOME, @SERIE, @DISCIPLINA
                ); SELECT SCOPE_IDENTITY();";

        private const string sqlEditar =
            @"UPDATE [tb_materia]
                SET
                    [NOME] = @NOME,
                    [SERIE] = @SERIE,
                    [DISCIPLINA] = @DISCIPLINA;
                WHERE 
                    [NUMERO] = @NUMERO";

        private const string sqlExcluir =
            @"DELETE FROM [tb_materia
                WHERE
                    [NUMERO] = @NUMERO";

        private const string sqlSelecionarTodos =
            @"SELECT [NUMERO], [NOME], [SERIE], [DISCIPLINA] FROM [tb_materia]";

        private const string sqlSelecionarPorNumero =
            @"SELECT [NUMERO], [NOME], [SERIE], [DISCIPLINA] FROM [tb_materia]
                WHERE
                    [NUMERO] = @NUMERO";


        public ValidationResult Inserir(Materia novaMateria)
        {
            var validador = new ValidadorMateria();

            var resultado = validador.Validate(novaMateria);

            if (!resultado.IsValid)
                return resultado;


            SqlConnection conexaoComBanco = new(enderecoBanco);

            SqlCommand comandoInsercao = new(sqlInserir, conexaoComBanco); // Aqui cria

            ConfigurarParametrosMateria(novaMateria, comandoInsercao);

            conexaoComBanco.Open();

            var id = comandoInsercao.ExecuteScalar(); // Aqui insere

            novaMateria.Numero = Convert.ToInt32(id);

            conexaoComBanco.Close();

            return resultado;
        }

        public ValidationResult Editar(Materia materia)
        {
            var validador = new ValidadorMateria();

            var resultado = validador.Validate(materia);

            if (!resultado.IsValid)
                return resultado;

            SqlConnection conexaoComBanco = new(enderecoBanco);

            SqlCommand comandoEdicao = new(sqlEditar, conexaoComBanco);

            ConfigurarParametrosMateria(materia, comandoEdicao);

            conexaoComBanco.Open();
            comandoEdicao.ExecuteNonQuery(); // Edita aqui
            conexaoComBanco.Close();

            return resultado;
        }

        public ValidationResult Excluir(Materia materiaSelecionada)
        {
            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);

            SqlCommand comandoExclusao = new SqlCommand(sqlExcluir, conexaoComBanco);

            comandoExclusao.Parameters.AddWithValue("NUMERO", materiaSelecionada.Numero);

            conexaoComBanco.Open();

            int numeroRegistrosExcluidos = comandoExclusao.ExecuteNonQuery(); // Exclui aqui

            var resultado = new ValidationResult();

            if (numeroRegistrosExcluidos == 0)
                resultado.Errors.Add(new ValidationFailure("", "Não deu pra deletar"));

            conexaoComBanco.Close();

            return resultado;
        }

        public List<Materia> SelecionarTodos()
        {
            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);

            SqlCommand comandoSelecao = new SqlCommand(sqlSelecionarTodos, conexaoComBanco);

            conexaoComBanco.Open();

            SqlDataReader leitor = comandoSelecao.ExecuteReader(); // Lê aqui

            List<Materia> materias = new List<Materia>();

            while (leitor.Read())
            {
                Materia materia = ConverterParaMateria(leitor);
                materias.Add(materia);
            }

            return materias;
        }


        public Materia SelecionarPorNumero(int numero)
        {
            SqlConnection conexaoComBanco = new SqlConnection(enderecoBanco);

            SqlCommand comandoSelecao = new SqlCommand(sqlSelecionarPorNumero, conexaoComBanco);

            comandoSelecao.Parameters.AddWithValue("NUMERO", numero);

            conexaoComBanco.Open();

            SqlDataReader leitor = comandoSelecao.ExecuteReader(); // Lê aqui

            Materia materia = new();

            if (leitor.Read())
                materia = ConverterParaMateria(leitor);

            conexaoComBanco.Close();

            return materia;
        }
        private Materia ConverterParaMateria(SqlDataReader leitor)
        {
            int numero = Convert.ToInt32(leitor["NUMERO"]);
            string nome = Convert.ToString(leitor["NOME"]);
            int serie = Convert.ToInt32(leitor["SERIE"]);

            return new Materia
            {
                Numero = numero,
                Nome = nome,
                Serie = (SerieEnum)serie,
            };
        }

        private void ConfigurarParametrosMateria(Materia materia, SqlCommand comando)
        {
            comando.Parameters.AddWithValue("NUMERO", materia.Numero);
            comando.Parameters.AddWithValue("NOME", materia.Nome);
            comando.Parameters.AddWithValue("SERIE", Convert.ToInt32(materia.Serie));
            comando.Parameters.AddWithValue("DISCIPLINA", materia.Disciplina.Numero);
        }
    }
}
