using ClassLibrary1;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using уп;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        private avtorizatia form;

        [TestInitialize]
        public void Setup()
        {
            // Инициализация формы перед каждым тестом
            form = new avtorizatia();
        }
       

        [TestMethod]
        public void WithValidCredentials_ShouldReturnTrue()
        {
            // Arrange
            form.UnblockUser(null, null);
            string validLogin = "login4"; // Замените на действительный логин
            string validPassword = "pass4"; // Замените на действительный пароль

            // Act
            bool result = form.ValidateUser(validLogin, validPassword);

            // Assert
            Assert.IsTrue(result, "Validation should return true for valid credentials.");
            form.UnblockUser(null, null);
        }

        [TestMethod]
        public void WithInvalidCredentials_ShouldReturnFalse()
        {
            // Arrange
            string invalidLogin = "invalidUser ";
            string invalidPassword = "invalidPassword";

            // Act
            bool result = form.ValidateUser(invalidLogin, invalidPassword);

            // Assert
            Assert.IsFalse(result, "Validation should return false for invalid credentials.");
            form.UnblockUser(null, null);
        }

        [TestMethod]
        public void ShouldSetIsBlockedToTrue()
        {
            // Act
            form.BlockUser();

            // Assert
            Assert.IsTrue(form.isBlocked, "User  should be blocked after BlockUser  is called.");
            form.UnblockUser(null, null);
        }

        [TestMethod]
        public void ShouldResetAttemptCountAndIsBlocked()
        {
            // Arrange
            form.BlockUser(); // Блокируем пользователя
            Assert.IsTrue(form.isBlocked, "User  should be blocked.");

            // Act
            form.UnblockUser(null, null);

            // Assert
            Assert.IsFalse(form.isBlocked, "User  should be unblocked after UnblockUser  is called.");
            Assert.AreEqual(0, form.attemptCount, "Attempt count should be reset to 0.");
            form.UnblockUser(null, null);
        }

        [TestMethod]
        public void UpdateAverageCompletionTime1_WithCompletedRequests_ShouldReturnAverageTime()
        {
            // Arrange
            DataTable requestsTable = new DataTable();
            requestsTable.Columns.Add("Дата начала", typeof(DateTime));
            requestsTable.Columns.Add("Дата завершения", typeof(DateTime));

            // Добавляем завершенные заявки
            requestsTable.Rows.Add(DateTime.Now.AddDays(-2), DateTime.Now); // 2 дня
            requestsTable.Rows.Add(DateTime.Now.AddDays(-1), DateTime.Now); // 1 день
            requestsTable.Rows.Add(DateTime.Now.AddDays(-3), DateTime.Now); // 3 дня

            // Act
            string result = Class1.UpdateAverageCompletionTime1(requestsTable);

            // Assert
            Assert.AreEqual("Среднее время выполнения: 2,0 дней", result);
        }

        [TestMethod]
        public void UpdateAverageCompletionTime1_WithoutCompletedRequests_ShouldReturnNoCompletedRequestsMessage()
        {
            // Arrange
            DataTable requestsTable = new DataTable();
            requestsTable.Columns.Add("Дата начала", typeof(DateTime));
            requestsTable.Columns.Add("Дата завершения", typeof(DateTime));

            // Добавляем незавершенные заявки (без даты завершения)
            requestsTable.Rows.Add(DateTime.Now.AddDays(-2), DBNull.Value);
            requestsTable.Rows.Add(DateTime.Now.AddDays(-1), DBNull.Value);

            // Act
            string result = Class1.UpdateAverageCompletionTime1(requestsTable);

            // Assert
            Assert.AreEqual("Среднее время выполнения: нет завершенных заявок", result);
        }
    }
}
