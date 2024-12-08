FROM ubuntu:16.04

RUN apt update
RUN apt install -y \
    make \
    gcc \
    socat

# add new user if it is needed
RUN useradd -d /home/ctf/ -m -p ctf -s /bin/bash ctf
RUN echo "ctf:ctf" | chpasswd

WORKDIR /home/ctf

COPY pwn_challenge.c .
COPY pwn_challenge .
COPY flag.txt .
COPY Makefile .

RUN chmod -R 755 /home/ctf
RUN chown -R root:root /home/ctf

# Compile binary
RUN make clean; make

# Change user
USER ctf

# Entry point
ENTRYPOINT socat tcp-l:8012,fork,reuseaddr exec:./pwn_challenge && /bin/bash