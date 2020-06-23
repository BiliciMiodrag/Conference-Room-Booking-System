let currentEvent;
const formatDate = date => date === null ? ' ' : moment(date).format("MM/DD/YYYY h:mm A");
const fpStartTime =

    flatpickr("#StartTime", {
    enableTime: true,
    dateFormat: "m/d/Y h:i K"
});
const fpEndTime = flatpickr("#EndTime", {
    enableTime: true,
    dateFormat: "m/d/Y h:i K"
});


$('#calendar').fullCalendar({
   
    defaultView: 'month',
    height: 'parent',
    header: {
        left: 'prev,next today',
        center: 'title',
        right: 'month,agendaWeek,agendaDay,listMonth'
    },
    editable: true,
    eventLimit: true,
    eventRender(event, $el) {
        $el.qtip({
            overwrite: false,
            content: {
                title: event.title,
                text: event.description + " " +"cu sala numărul " + event.salaID
            },
            hide: {
                event: 'unfocus'
            },
            show: {
                solo: true
            },
            position: {
                my: 'top left',
                at: 'bottom left',
                viewport: $('#calendar-wrapper'),
                adjust: {
                    method: 'shift'
                }
            }
        });
    },
    events: '/Calendar/GetCalendarEvents',
    eventClick: updateEvent,
    selectable: true,
    select: addEvent,
    
});

/**
 *  Metode pentru calendar
 **/

function updateEvent(event, element) {
    currentEvent = event;

    if ($(this).data("qtip")) $(this).qtip("hide");

    $('#eventModalLabel').html('Editare Eveniment');
    $('#eventModalSave').html('Actualizare');
    $('#EventTitle').val(event.title);
    $('#Description').val(event.description);
    $('#Color').val(event.color);
    $('#salaID').val(event.salaID);
    


    $('#isNewEvent').val(false);

    const start = formatDate(event.start);
    const end = formatDate(event.end);

    fpStartTime.setDate(start);
    fpEndTime.setDate(end);

    $('#StartTime').val(start);
    $('#EndTime').val(end);

    if (event.allDay) {
        $('#AllDay').prop('checked', 'checked');
    } else {
        $('#AllDay')[0].checked = false;
    }

    $('#eventModal').modal('show');
}

function addEvent(start, end) {
    $('#eventForm')[0].reset();

    $('#eventModalLabel').html('Adaugă Eveniment');
    $('#eventModalSave').html('Creează Eveniment');
    $('#isNewEvent').val(true);

    start = formatDate(start);
    end = formatDate(end);

    fpStartTime.setDate(start);
    fpEndTime.setDate(end);

    $('#eventModal').modal('show');
}

/**
 * Modal
 * */

$('#eventModalSave').click(() => {
    const title = $('#EventTitle').val();
    const description = $('#Description').val();
    const startTime = moment($('#StartTime').val());
    const endTime = moment($('#EndTime').val());
    const color = ($('#Color').val());
    salaID = ($('#salaID').val());
    var roomID = parseInt(salaID);
    //const UserId = ($('#ApplicationUserId').val());
    const isAllDay = $('#AllDay').is(":checked");
    const isNewEvent = $('#isNewEvent').val() === 'true' ? true : false;

    if (startTime > endTime) {
        alert('Ora de sfărșit a evenimentului nu poate fi mai mare decât ora de începere');

        return;
    } else if ((!startTime.isValid() || !endTime.isValid()) && !isAllDay) {
        alert('Vă rugăm să introduceți ora de început și sfărșit a evenimentului');

        return;
    }

    const event = {
        title,
        description,
        isAllDay,
        startTime: startTime._i,
        endTime: endTime._i,
        color,
        roomID,
    };
   
        
    

    if (isNewEvent) {
        sendAddEvent(event);
    } else {
        sendUpdateEvent(event);
    }
});

function sendAddEvent(event) {
    axios({
        method: 'post',
        url: '/Calendar/AddEvent',
        data: {
            "Title": event.title,
            "Description": event.description,
            "Start": event.startTime,
            "End": event.endTime,
            "AllDay": event.isAllDay,
            "color": event.color,
            salaID: event.roomID,
            //"UserId": UserId,
        }
    })
        .then(res => {
            const { message, eventId } = res.data;

            if (message === '') {
                const newEvent = {
                    start: event.startTime,
                    end: event.endTime,
                    allDay: event.isAllDay,
                    title: event.title,
                    description: event.description,
                    color: event.color,
                    salaID: event.roomID,
                  //  UserId: UserId,
                    
                    eventId
                };

                $('#calendar').fullCalendar('renderEvent', newEvent);
                $('#calendar').fullCalendar('unselect');

                $('#eventModal').modal('hide');
            } else {
                alert(`Something went wrong: ${message}`);
            }
        })
        .catch(err => alert(`Something went wrong: ${err}`));
}

function sendUpdateEvent(event) {
    axios({
        method: 'post',
        url: '/Calendar/UpdateEvent',
        data: {
            "EventId": currentEvent.eventId,
            "Title": event.title,
            "Description": event.description,
            "Start": event.startTime,
            "End": event.endTime,
            "AllDay": event.isAllDay,
            "color": event.color,
            "salaID": event.roomID,
           
          
        }
    })
        .then(res => {
            const { message } = res.data;

            if (message === '') {
                currentEvent.title = event.title;
                currentEvent.description = event.description;
                currentEvent.start = event.startTime;
                currentEvent.end = event.endTime;
                currentEvent.allDay = event.isAllDay;
                currentEvent.color = event.color;
                currentEvent.salaID = event.salaID;
               // currentEvent.UserId = UserId;

                $('#calendar').fullCalendar('updateEvent', currentEvent);
                $('#eventModal').modal('hide');
            } else {
                alert(`Something went wrong: ${message}`);
            }
        })
        .catch(err => alert(`Something went wrong: ${err}`));
}

$('#deleteEvent').click(() => {
    if (confirm(`Doriți să stergeți "${currentEvent.title}" acest eveniment?`)) {
        axios({
            method: 'post',
            url: '/Calendar/DeleteEvent',
            data: {
                "EventId": currentEvent.eventId
            }
        })
            .then(res => {
                const { message } = res.data;

                if (message === '') {
                    $('#calendar').fullCalendar('removeEvents', currentEvent._id);
                    $('#eventModal').modal('hide');
                } else {
                    alert(`Ceva a funcționat gresit: ${message}`);
                }
            })
            .catch(err => alert(`Ceva a funcționat gresit: ${err}`));
    }
});

$('#AllDay').on('change', function (e) {
    if (e.target.checked) {
        $('#EndTime').val('');
        fpEndTime.clear();
        this.checked = true;
    } else {
        this.checked = false;
    }
});

$('#EndTime').on('change', () => {
    $('#AllDay')[0].checked = false;
});
