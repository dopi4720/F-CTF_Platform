import os
from flask import Flask, request, redirect, url_for, render_template, session, send_from_directory
from werkzeug.middleware import proxy_fix
from database import init, search_answer, get_answer_by_question, insert_question, register_user, authenticate_user
from util import find_most_similar_question
from flask_session import Session  
from datetime import timedelta
import shutil


app = Flask(__name__)
app.wsgi_app = proxy_fix.ProxyFix(app.wsgi_app)
# Secret key for sessions encryption. Set this to a random string.
app.secret_key = os.urandom(24)

#clear_session
folder = './flask_session'
shutil.rmtree(folder, ignore_errors=True)

# Configure session to use filesystem (instead of signed cookies)
app.config['SESSION_TYPE'] = 'filesystem'
Session(app)
questions = []



# @app.before_request
# def clear_filesystem_sessions():
#     app.before_request_funcs[None].remove(clear_filesystem_sessions)    


@app.route('/', methods=['GET'])
def index():
    if 'user_id' in session:
        return render_template('index.html')
    else:
        return redirect(url_for('login'))


@app.route('/', methods=['POST'])
def query():
    if 'user_id' in session:
        answer = ''
        search_term = request.form['search']
        advance = request.form['advance']
        
        if ')' in search_term or '(' in search_term:
            answer = 'No parenthesis allow! Please phase your question clearly without parenthesis!'
        elif len(search_term) >= 150:
            answer = 'Why too long? You should take a deep breath, look inside yourself to get out a short and clear question!'
        elif 'PG_SLEEP' in search_term:
            answer = 'I never sleep! A mindfulness person always what he/she is doing!'
        else:
            if advance:
                most_similar_question = find_most_similar_question(search_term, session['user_id'])
                result = get_answer_by_question(most_similar_question, session['user_id'])
            else:
                result = search_answer(search_term, session['user_id'])

            if result:
                answer = result
            else:
                answer = "I do not find it inside yourself. Keep calm, meditate and let the silence tell you the answer. After deep zen time, you could answer the question by yourself."

        return render_template('index.html', answer = answer, checked = advance)
    else:
        return redirect(url_for('login'))


@app.route('/login', methods=['GET', 'POST'])
def login():
    if request.method == 'POST':
        username = request.form['username']
        password = request.form['password']

        # Authenticate the user
        user_id = authenticate_user(username, password)
        
        if user_id:
            # Set the user session if authentication is successful
            session['user_id'] = user_id
            return redirect(url_for('index'))
        else:
            return "Login failed. Please try again.", 401

    register_status = None
    if '_register_status' in session:
        register_status = session['_register_status']
        session.pop('_register_status', None)

    register_message = ''
    if '_register_message' in session:
        register_message = session['_register_message']
        session.pop('_register_message', None)
    
    return render_template('login.html', register_status = register_status, register_message = register_message)


@app.route('/register', methods=['GET', 'POST'])
def register():
    register_status = None
    register_message = ''

    if request.method == 'POST':
        username = request.form['username']
        password = request.form['password']

        register_status, register_message = register_user(username, password)
        
        if register_status:
            session['_register_status'] = register_status
            session['_register_message'] = register_message
            # Redirect to the login page after successful registration
            return redirect(url_for('login'))
    
    # Show the registration form again if registration fails
    return render_template('register.html', register_status = register_status, register_message = register_message)


@app.route('/logout')
def logout():
    # Remove user_id from session
    session.pop('user_id', None)
    return redirect(url_for('login'))


@app.route('/answer', methods=['GET'])
def answer():
    if 'user_id' in session:
        return render_template('answer.html')
    else:
        return redirect(url_for('login'))


@app.route('/answer', methods=['POST'])
def app_insert_question():
    if 'user_id' in session:
        question = request.form['question']
        answer = request.form['answer']
        
        if insert_question(question, answer, session['user_id']):
            return render_template('answer.html', update_status = True, update_message = 'Your inside universe is widen!')
        else:
            return render_template('answer.html', update_status = False, update_message = 'Something went wrong! Please try to create another account, then login again.')
    else:
        return redirect(url_for('login'))

@app.route('/er_diagram')
def er_diagram():
    return send_from_directory(directory='./', path='challenge.erd', as_attachment=True)

def main():
    init()
    app.run(host='0.0.0.0', port=8000, debug=False)


if __name__ == "__main__":
    main()