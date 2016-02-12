# Задание с выбором подсказки #

  * Создаем новое задание.
  * В свойстве **Тип задания** указываем значение **2**, т.е.  задание с выбором подсказки.
  * Сохраняем созданное задание.

Задание создано. Теперь необходимо задать текст задания и создать подсказки для задания.
Дальнейшие действия практически ничем не отличаются от создания классического задания, но все же опишу процесс заполнения задания.

Для указания текста задания и создания подсказок находим в списке заданий созданное нами заданий и переходим по ссылке **изменить**

### Задание текста задания ###

  * Под надписью _Здесь должен быть текст задания..._ переходим по ссылке **изменить**.
  * В текстовом поле задаем текст задания. Поле **Время** оставляем неизменным. Значение **0** в поле **Время** указывает, что команда получит текст задания незамедлительно, когда команде буден назначено задание.
  * Сохраняем изменения.

### Создание вариантов подсказок ###

Допустим, что нам требуется создать задание с возможностью выбора любых двух подсказок из трех возможных, а выбор подсказки должен быть предложен команде спустя 30 минут и 60 минут после получения задания.

Для того, чтобы выбор подсказки был доступен через 30 и 60 минут, необходимо для любых двух подсказок в поле **Время** указать значения 30 и 60. А для оставшейся подсказки необходимо указать в поле **Время** значение большее или равное количества минут выделенного на выполнение задания - это предотвратит возможность выбора последней подсказки. В данном случае, значения указанные в поле **Время** для подсказок, просто определяют моменты времени, когда необходимо будет предложить команде выбор подсказки, а не время прихода конкретной подсказки.

Соответственно для того, чтоб создать варианты подсказок нужно просто создать три подсказки не изменяя значение в поле **Время** для каждой подсказки. Таким образом мы получим три подсказки, в которых значения в поле **Время** соответственно будут 30, 60 и 90 минут.

Вот вроде и все.