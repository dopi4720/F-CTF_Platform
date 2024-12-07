import psycopg2
from psycopg2.errors import CollationMismatch
from psycopg2.errors import SyntaxError as PSQLSyntaxError
from werkzeug.security import check_password_hash
from werkzeug.security import generate_password_hash
from passsword import generate_secure_password


conn = psycopg2.connect(
        dbname="postgres",
        user="postgres",
        password="postgres",
        host="localhost",
        port="5432"
    )


def authenticate_user(username, password):
    # Open a cursor to perform database operations
    cur = conn.cursor()

    # Method to find the user with provided credentials
    try:
        # Query to find the user
        cur.execute("SELECT id, password FROM users WHERE username = %s", (username,))
        user_record = cur.fetchone()
        
        if user_record:
            user_id, hashed_password = user_record
            # Check if the provided password matches the stored hashed password
            if check_password_hash(hashed_password, password):
                return user_id  # Username and password match; return user's id
            # if password == hashed_password:
            #     return user_id
            else:
                return None  # Password does not match
        else:
            return None  # No user found with the given username
    except Exception as e:
        print(f"An error occurred: {e}")
        return None
    finally:
        cur.close()


# Function to register a user with a hashed password
def register_user(username, plaintext_password):
    cur = conn.cursor()
    try:
        hashed_password = generate_password_hash(plaintext_password)
        
        # Insert the new user into the database
        cur.execute(
            "INSERT INTO users (username, password) VALUES (%s, %s)", 
            (username, hashed_password)
        )
        
        # Commit the changes
        conn.commit()
        return True, "User registered successfully!"
    except psycopg2.errors.UniqueViolation:
        return False, "Registration failed: Username already exists."
    except Exception as e:
        return False, f"An error occurred: {e}"
    finally:
        # Close the cursor and connection
        cur.execute("ROLLBACK")
        cur.close()


def get_all_questions_of_user(user_id):
    # Create a cursor object
    cur = conn.cursor()

    # Execute the SQL query
    cur.execute("""SELECT question FROM inside_intelligence WHERE user_id IN (1,%s)""", (user_id, ))

    # Fetch all rows from the table
    result = cur.fetchall()
    questions = [r[0] for r in result]
    del result

    #Close cursor and connection
    cur.close()
    
    return questions



def search_answer(search_term, user_id):
    # Create a cursor object
    cur = conn.cursor()

    answer = None
    # Search for relevant question
    try:
        cur.execute(f"""
            SELECT answer 
            FROM inside_intelligence 
            WHERE user_id IN (%s, 1)
            AND question ILIKE %s COLLATE "und-u-co-emoji-x-icu"
            LIMIT 1""", 
        (user_id, f"%{search_term}%"))
        result = cur.fetchone()
        if result:
            answer = result[0]
    except CollationMismatch:
        answer = "Collation Mismatch"
    except PSQLSyntaxError:
        answer = "Syntax Error"
    except Exception:
        answer = "Unknown Exception"

    # Close cursor and connection
    cur.close()
    
    return answer


def get_answer_by_question(question, user_id):
    cur = conn.cursor()

    answer = ''
    # Search for relevant question
    try:
        cur.execute(f"""
            SELECT answer 
            FROM inside_intelligence 
            WHERE user_id IN (%s, 1)
            AND question = '{question}' LIMIT 1""",
        (user_id, ))
        result = cur.fetchone()
        if result:
            answer = result[0]
    except CollationMismatch:
        answer = "Collation Mismatch"
    except PSQLSyntaxError:
        answer = "Syntax Error"
    except Exception:
        answer = "Unknown Exception"
    finally:
        # Close cursor and connection
        cur.execute("ROLLBACK")
        cur.close()

    return answer


def insert_question(question, answer, user_id):
    cur = conn.cursor()

    try:
        cur.execute("INSERT INTO inside_intelligence (question, answer, user_id) VALUES (%s, %s, %s)", (question, answer, user_id))
        # Commit changes
        conn.commit()
        return True
    except Exception:
        return False
    finally:
        cur.execute("ROLLBACK")
        cur.close()
    


def init():
    # Create a cursor object for "postgres" database connection
    cur = conn.cursor()

    #Database cleaning
    cur.execute("""
        DROP TABLE IF EXISTS inside_intelligence
    """)

    cur.execute("""
        DROP TABLE IF EXISTS users
    """)

    cur.execute("""
        DROP TABLE IF EXISTS flag
    """)

    # Execute a query to create the table if it doesn't exist
    cur.execute(
        """
        CREATE TABLE IF NOT EXISTS users (
            id SERIAL PRIMARY KEY, 
            username VARCHAR(255) UNIQUE NOT NULL, 
            password VARCHAR(255) NOT NULL
        )
        """
    )

    register_user("anonymous", generate_secure_password(12))

    # Create table inside_intelligence with collation

    cur.execute("""
        CREATE COLLATION IF NOT EXISTS "und-u-co-emoji-x-icu" (provider = icu, locale = 'und-u-co-emoji');;
    """)
    
    cur.execute("""
        CREATE TABLE IF NOT EXISTS inside_intelligence (
            id SERIAL PRIMARY KEY,
            question TEXT NOT NULL COLLATE "und-u-co-emoji-x-icu",
            answer TEXT NOT NULL COLLATE "und-u-co-emoji-x-icu" ,
            user_id INTEGER NOT NULL,
            FOREIGN KEY (user_id) REFERENCES users(id)
        )
    """)

    # Sample questions and answers
    questions_answers = [
        ("What are your core values?", "My core values include honesty, integrity, and compassion.", 1),
        ("What motivates you?", "I am motivated by the desire to learn, grow, and make a positive impact. Find out more about myself at /er_diagram.", 1),
        ("How do you handle failure?", "I see failure as an opportunity to learn and improve.", 1),
        ("What are your strengths and weaknesses?", "My strengths include adaptability and problem-solving skills, while I'm working on improving my time management.",1),
        ("How do you define success?", "Success, to me, is achieving personal and professional goals while maintaining a sense of fulfillment and happiness.", 1)
    ]

    # Insert data into the inside_intelligence table
    for question, answer, user_id in questions_answers:
        insert_question(question, answer, user_id)

    # Create table flag

    cur.execute("""
        CREATE TABLE IF NOT EXISTS flag (
            text TEXT COLLATE "unicode"
        )
    """)

    # Insert value into flag table
    cur.execute("INSERT INTO flag (text) VALUES ('FUSEC{hope_you_already_know_about_collation_in_PGSQL}')")

    # Commit changes
    conn.commit()

    cur.close()