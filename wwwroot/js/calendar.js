let currentEvent;
const formatDate = date => date === null ? ' ' : moment(date).format("MM/DD/YYYY h:mm A");
const fpStartTime = flatpickr("#StartTime", {
    enableTime: true,
    dateFormat: "m/d/Y h:i K"
});
const fpEndTime = flatpickr("#EndTime", {
    enableTime: true,
    dateFormat: "m/d/Y h:i K"
});


/*document.addEventListener('DOMContentLoaded', function () {


    var calendarEl = document.getElementById('calendar');

    var calendar = new FullCalendar.Calendar(calendarEl, {
        plugins: ['interaction', 'resourceDayGrid', 'resourceTimeGrid'],
        defaultView: 'dayGridMonth',
        schedulerLicenseKey: 'GPL-My-Project-Is-Open-Source',
        editable: true,
        selectable: true,
        eventLimit: true, // allow "more" link when too many events
        header: {
            left: 'prev,next today',
            center: 'title',
            right: 'resourceTimeGridDay,resourceTimeGridTwoDay,,timeGridWeek,dayGridMonth'
        },
        views: {
            resourceTimeGridTwoDay: {
                type: 'resourceTimeGrid',
                duration: { days: 2 },
                buttonText: '2 days',
            }
        },

        //// uncomment this line to hide the all-day slot
        //allDaySlot: false,

        eventRender(event, $el) {
            $el.qtip({
                content: {
                    title: event.title,
                    text: event.description
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
        select: addEvent


       
    });

    calendar.render();
});

*/

$('#calendar').fullCalendar({
    defaultView: 'month',
    height: 'parent',
    header: {
        left: 'prev,next today',
        center: 'title',
        right: 'month,agendaWeek,agendaDay'
    },
    eventRender(event, $el) {
        $el.qtip({
            content: {
                title: event.Titlu,
                text: event.Descriere
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

    $('#eventModalLabel').html('Edit Event');
    $('#eventModalSave').html('Update Event');
    $('#EventTitle').val(event.Titlu);
    $('#Description').val(event.Descriere);
    $('#Color').val(event.culoare);
    $('#RoomId').val(event.roomID);
    $('#Participants').val(event.participanti);


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

    $('#eventModalLabel').html('Add Event');
    $('#eventModalSave').html('Create Event');
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
    const Titlu = $('#EventTitle').val();
    const Descriere = $('#Description').val();
    const startTime = moment($('#StartTime').val());
    const endTime = moment($('#EndTime').val());
    const culoare = moment($('#Color').val());
    const roomID = moment($('#RoomID').val());
    const participants = moment($('#Participants').val());
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
        Titlu,
        Descriere,
        isAllDay,
        startTime: startTime._i,
        endTime: endTime._i,
        culoare,
        roomID,
        participants
        
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
            "Titlu": event.Titlu,
            "Descriere": event.Descriere,
            "start_data": event.startTime,
            "sfarsit_data": event.endTime,
            "AllDay": event.isAllDay,
            "culoare": event.culoare,
            "salaID": event.salaID,
            "participanti": event.participanti
        }
    })
        .then(res => {
            const { message, eventId } = res.data;

            if (message === '') {
                const newEvent = {
                    start: event.startTime,
                    end: event.endTime,
                    allDay: event.isAllDay,
                    title: event.Titlu,
                    description: event.Descriere,
                    culoare: event.culoare,
                    salaID: event.salaID,
                    participanti: event.participanti,
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
            "ID": currentEvent.eventId,
            "Titlu": event.Titlu,
            "Descriere": event.Descriere,
            "start_data": event.startTime,
            "sfarsit_data": event.endTime,
            "AllDay": event.isAllDay,
            "culoare": event.culoare,
            "salaID": event.salaID,
            "participanti": event.participanti
        }
    })
        .then(res => {
            const { message } = res.data;

            if (message === '') {
                currentEvent.title = event.Titlu;
                currentEvent.description = event.Descriere;
                currentEvent.start = event.startTime;
                currentEvent.end = event.endTime;
                currentEvent.allDay = event.isAllDay;
                currentEvent.culoare = event.culoare;
                currentEvent.salaID = event.salaID;
                currentEvent.participanti = event.participanti;

                $('#calendar').fullCalendar('updateEvent', currentEvent);
                $('#eventModal').modal('hide');
            } else {
                alert(`Something went wrong: ${message}`);
            }
        })
        .catch(err => alert(`Something went wrong: ${err}`));
}

$('#deleteEvent').click(() => {
    if (confirm(`Do you really want to delte "${currentEvent.Titlu}" event?`)) {
        axios({
            method: 'post',
            url: '/Calendar/DeleteEvent',
            data: {
                "ID": currentEvent.eventId
            }
        })
            .then(res => {
                const { message } = res.data;

                if (message === '') {
                    $('#calendar').fullCalendar('removeEvents', currentEvent._id);
                    $('#eventModal').modal('hide');
                } else {
                    alert(`Something went wrong: ${message}`);
                }
            })
            .catch(err => alert(`Something went wrong: ${err}`));
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
